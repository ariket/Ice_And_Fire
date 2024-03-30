using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using System.ComponentModel.Design;
using System.Xml.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks.Dataflow;


namespace IceAndFireAPIExample
{
    // a class for books in "Ice and Fire"
    public class Book
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("url")]
        public string? Url { get; set; }

        [JsonProperty("characters")]
        public List<string>? Characters { get; set; }

        [JsonProperty("povCharacters")]
        public List<string>? PovCharacters { get; set; }
    }

    // a class for houses in "Ice and Fire"
    public class House
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("url")]
        public string? Url { get; set; }

        [JsonProperty("swornMembers")]
        public List<string>? SwornMembers { get; set; }
    }

    // A class for characters in "Ice and Fire"
    public class Character : IComparable<Character>
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "unknown";

        [JsonProperty("url")]
        public string? Url { get; set; }

        public string? HouseName { get; set; } = "unknown";

        public void SetHouseName(string houseName)
        {
            this.HouseName = houseName;
        }

        [JsonProperty("titles")]
        public List<string>? Titles { get; set; }

        [JsonProperty("books")]
        public List<string>? Books { get; set; }

        [JsonProperty("povBooks")]
        public List<string>? PovBooks { get; set; }

        public int CompareTo(Character? other)
        {
            return Name.CompareTo(other!.Name);
        }

        public override string ToString()
        {
            return $"{Name,-16} {HouseName,-28} ";
        }

    }

    class Program
    {
        //"bookTitlesToUse"/"bookUrlToUse" are specified in the assignment
        public static string[] bookTitlesToUse = { "A Game of Thrones", "A Clash of Kings", "A Storm of Swords", "A Feast for Crows", "A Dance with Dragons" };
        //public static string[] bookUrlToUse = { "https://www.anapioficeandfire.com/api/books/1", "https://www.anapioficeandfire.com/api/books/2", "https://www.anapioficeandfire.com/api/books/3", "https://www.anapioficeandfire.com/api/books/5", "https://www.anapioficeandfire.com/api/books/8" };
        public static List<Book> bookToUse = new List<Book>();
        public static List<Character> SwornMembers = new List<Character>();


        static async Task Main(string[] args)
        {
            _ = getBooksFromServer();

            _ = getHousesFromServer();


            using (HttpClient client = new HttpClient()) // Create a HttpClient-instance to be able to call the API
            {
                try
                {

                    // GET to fetch data about "House Arryn of the Eyrie"
                    HttpResponseMessage responseArryn = await client.GetAsync("https://www.anapioficeandfire.com/api/houses/7");
                    //HttpResponseMessage responseArryn = await client.GetAsync("https://www.anapioficeandfire.com/api/houses/?name=House%20Arryn%20of%20the%20Eyrie");
                    responseArryn.EnsureSuccessStatusCode(); // Throw exception if no response from server
                    string responseDataArryn = await responseArryn.Content.ReadAsStringAsync();
                    //responseDataArryn = responseDataArryn[1..^1]; //tar bort förta och sista tecknet: [ ]
                    House arrynHouse = JsonConvert.DeserializeObject<House>(responseDataArryn)!;
                   
                    // GET to fetch data about "House Baelish of the Fingers"
                    HttpResponseMessage responseBaelish = await client.GetAsync("https://www.anapioficeandfire.com/api/houses/11");
                    responseBaelish.EnsureSuccessStatusCode(); // Throw exception if no response from server
                    string responseDataBaelish = await responseBaelish.Content.ReadAsStringAsync();
                    House baelishHouse = JsonConvert.DeserializeObject<House>(responseDataBaelish)!;

                    // Get swornMembers for the two houses specified in the assignment
                    SwornMembers = await GetSwornMembers(client, arrynHouse.SwornMembers!, "House Arryn of the Eyrie");
                    List<Character> baelishSwornMembers = await GetSwornMembers(client, baelishHouse.SwornMembers!, "House Baelish of the Fingers");
                    SwornMembers.AddRange(baelishSwornMembers);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Error: {e.Message}");
                }
            }


            Console.WriteLine("--------------------------------------------------------------------------------------------------------");
                    Console.WriteLine("|                House Arryn of the Eyrie and House Baelish of the Fingers Sworn Members               |");
                    Console.WriteLine("--------------------------------------------------------------------------------------------------------\n");

                    foreach (Book book in bookToUse)
                    {
                        Console.WriteLine("--------------------------------------------------------------------------------------------------------");
                        Console.WriteLine($"|                Book title: {book.Name}                                                         |");
                        Console.WriteLine("--------------------------------------------------------------------------------------------------------");
                        Console.WriteLine("Charactername:   HouseName:                    Titles:");
                        Console.WriteLine("Appears also in these books:");
                        Console.WriteLine("--------------------------------------------------------------------------------------------------------\n");
                        foreach (Character character in SwornMembers)
                        {
                            foreach (string bookUrl in character.Books!)
                                if (bookUrl == book.Url)
                                {
                                    Console.Write(character);
                                    Console.WriteLine(listAllTitles(character, book));
                                }
                        }
                        Console.WriteLine();
                    }
          
        }

        public static async Task getBooksFromServer()
        {
            foreach (string bookTitle in bookTitlesToUse) // Create a HttpClient-instance to be able to call the API
            {
                using (HttpClient client = new HttpClient())
                {
                    try
                    {

                        // GET to fetch data about books specified in the assignment
                        HttpResponseMessage responseBook = await client.GetAsync($"https://www.anapioficeandfire.com/api/books/?name={bookTitle}");
                        responseBook.EnsureSuccessStatusCode(); // Throw exception if no response from server
                        string responseDataBook = await responseBook.Content.ReadAsStringAsync();
                        responseDataBook = responseDataBook[1..^1]; //Removes first and last char from GET answer: [ ]
                        bookToUse.Add(JsonConvert.DeserializeObject<Book>(responseDataBook)!);
                    }
                    catch (HttpRequestException e)
                    {
                        Console.WriteLine($"Error: {e.Message}");
                    }
                }
            }
        }

        public static async Task getHousesFromServer()
        {


        }


            public static string listAllTitles(Character character, Book book) //Lists all titles of the character
        {
            string ReturnString = "";
            int numOfTitles = 0;
            foreach (string title in character.Titles!)
            {
                if (numOfTitles > 0)
                    ReturnString += ", ";
               
               ReturnString += $"{title,1}";
                
               numOfTitles++;
            }   
            listAllBooks(character, ref ReturnString, book);

            return ReturnString;
        }

        public static string listAllBooks(Character character, ref string ReturnString, Book book) //Lists all books where character appears
        {
            var bookQuery =       
                from books in character.Books
                join bookk in bookToUse
                    on books equals bookk.Url
                where books != book.Url
                select bookk.Name;

            int numOfBooks = 0;
            foreach (string books in bookQuery) 
            {
                if (bookTitlesToUse.Contains(books)) //only list books that appears in the "bookTitlesToUse" array - "bookTitlesToUse" are specified in the assignment
                {
                    if (numOfBooks > 0)
                        ReturnString += ", ";
                    else
                        ReturnString += "\n";

                    ReturnString += $"{books,1}";
                }
                numOfBooks++;
            }
            listAllPOV(character, ref ReturnString, book);

            return ReturnString;
        }

        public static string listAllPOV(Character character, ref string ReturnString, Book book) //Lists all books where the character is POV(Point of view-person)
        {
            var bookPOVQuery =
                from povBooks in character.PovBooks
                join bookk in bookToUse
                    on povBooks equals bookk.Url
                select bookk.Name;

            int numOfBooks = 0;
            foreach (string povBook in bookPOVQuery)
            {
                if (bookTitlesToUse.Contains(povBook)) //only list books that appears in the "bookTitlesToUse" array - "bookTitlesToUse" are specified in the assignment
                {
                    if (numOfBooks > 0)
                        ReturnString += ", ";
                    else
                        ReturnString += "\nPOV-person in Books: ";

                    ReturnString += $"{povBook,1}";
                }
                numOfBooks++;
            }
            ReturnString += "\n";

            return ReturnString;
        }

        // Function to fetch swornMembers from URL
        static async Task<List<Character>> GetSwornMembers(HttpClient client, List<string> swornMemberUrls, String CurrentHouseName)
        {
            List<Character> swornMemberCharacters = new List<Character>();

            foreach (string swornMemberUrl in swornMemberUrls)
            {
                if (!string.IsNullOrEmpty(swornMemberUrl))
                {
                    HttpResponseMessage response = await client.GetAsync(swornMemberUrl);
                    response.EnsureSuccessStatusCode();
                    string responseData = await response.Content.ReadAsStringAsync();
                    Character swornMemberData = JsonConvert.DeserializeObject<Character>(responseData)!;
                    swornMemberCharacters.Add(swornMemberData);
                    int elementNum = swornMemberCharacters.Count();
                    swornMemberCharacters[elementNum-1].SetHouseName(CurrentHouseName);
                }
            }

            //remove all books that aren't in the "bookUrlToUse" array - "bookUrlToUse" are specified in the assignment
         //   foreach (Character character in swornMemberCharacters!)
         //   {
         //       foreach (string book in character.Books!.ToList())
         //       {
         //           if (!bookUrlToUse.Contains(book))
         //               character.Books!.Remove(book);
         //       }

         //   }

            swornMemberCharacters.Sort();
            return swornMemberCharacters;
        }
    }
}
