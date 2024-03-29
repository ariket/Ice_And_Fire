using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using System.ComponentModel.Design;
using System.Xml.Linq;


namespace IceAndFireAPIExample
{
    
    class Program
    {
        public static string[] bookTitlesToUse = { "A Game of Thrones", "A Clash of Kings", "A Storm of Swords", "A Feast for Crows", "A Dance with Dragons" };
        public static string[] bookUrlToUse = { "https://anapioficeandfire.com/api/books/1", "https://www.anapioficeandfire.com/api/books/2", "https://www.anapioficeandfire.com/api/books/3", "https://www.anapioficeandfire.com/api/books/5", "https://www.anapioficeandfire.com/api/books/8" };


        static async Task Main(string[] args)
        {
            List<Book> bookToUse = new List<Book>();
           
            foreach (string bookTitle in bookTitlesToUse) 
            {
                using (HttpClient client = new HttpClient())
                {
                    try
                    {

                        // Gör en GET-förfrågan för att hämta data om böckerna"
                        HttpResponseMessage responseBook = await client.GetAsync($"https://www.anapioficeandfire.com/api/books/?name={bookTitle}");
                        responseBook.EnsureSuccessStatusCode(); // Kastar ett undantag om förfrågan misslyckas
                        string responseDataBook = await responseBook.Content.ReadAsStringAsync();
                        responseDataBook = responseDataBook[1..^1]; //tar bort första och sista tecknet: [ ]
                        Book arrynHouse = JsonConvert.DeserializeObject<Book>(responseDataBook)!;
                        bookToUse.Add(arrynHouse);
                    }
                    catch (HttpRequestException e)
                    {
                        Console.WriteLine($"Error: {e.Message}");
                    }
                }
            }

            // Skapa en HttpClient-instans för att göra förfrågningar till API:et
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    
                    // Gör en GET-förfrågan för att hämta data om adelshuset "House Arryn of the Eyrie"
                    HttpResponseMessage responseArryn = await client.GetAsync("https://www.anapioficeandfire.com/api/houses/7");
                    //HttpResponseMessage responseArryn = await client.GetAsync("https://www.anapioficeandfire.com/api/houses/?name=House%20Arryn%20of%20the%20Eyrie");
                    responseArryn.EnsureSuccessStatusCode(); // Kastar ett undantag om förfrågan misslyckas
                    string responseDataArryn = await responseArryn.Content.ReadAsStringAsync();
                    //responseDataArryn = responseDataArryn[1..^1]; //tar bort förta och sista tecknet: [ ]
                    House arrynHouse = JsonConvert.DeserializeObject<House>(responseDataArryn)!;
                   
                    // Gör en GET-förfrågan för att hämta data om adelshuset "House Baelish of the Fingers"
                    HttpResponseMessage responseBaelish = await client.GetAsync("https://www.anapioficeandfire.com/api/houses/11");
                    responseBaelish.EnsureSuccessStatusCode(); // Kastar ett undantag om förfrågan misslyckas
                    string responseDataBaelish = await responseBaelish.Content.ReadAsStringAsync();
                    House baelishHouse = JsonConvert.DeserializeObject<House>(responseDataBaelish)!;

                    // Hämta swornMembers från respektive hus
                    List<Character> SwornMembers = await GetSwornMembers(client, arrynHouse.SwornMembers!, "House Arryn of the Eyrie");
                    List<Character> baelishSwornMembers = await GetSwornMembers(client, baelishHouse.SwornMembers!, "House Baelish of the Fingers");
                    SwornMembers.AddRange(baelishSwornMembers);
                    
                    
                    // Skriv ut swornMembers för varje hus
                    Console.WriteLine("House Arryn of the Eyrie and House Baelish of the Fingers Sworn Members:");
                    Console.Write("Name:            HouseName:                    Titles:\n");
                    foreach (Character Ch in SwornMembers)
                    {
                        Console.WriteLine(Ch);
                    }
                    Console.WriteLine();

                    Console.WriteLine("House Baelish of the Fingers Sworn Members:");
                    foreach (Character Ch in baelishSwornMembers)
                    {
                        Console.Write(Ch.Name);
                        Console.WriteLine($" {Ch.Books![0]}  {Ch.HouseName}");
                        Ch.HouseName = "House Baelish of the Fingers";
                    }
                    Console.WriteLine();
                    
                    // Här skulle du jämföra swornMembers och se om någon är med i samma bok

                    // Book book = bookToUse
                    //     .Where(book.Url => book.Url.Contains());

                    foreach (Book book in bookToUse)
                    {
                        Console.WriteLine(book.Name);
                        foreach (Character character in SwornMembers)
                        {
                            foreach (string bookUrl in character.Books!)
                                if (bookUrl == book.Url)
                                {
                                    Console.Write(character);
                                    Console.WriteLine(listAllTitlesAndAllBooks(character));
                                }
                        }
                        Console.WriteLine();
                    }


                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Error: {e.Message}");
                }

            }

        }

        public static string listAllTitlesAndAllBooks(Character character)
        {

            string ReturnString = "";
            int numOfTitles = 0;
            foreach(string title in character.Titles!)
            {
                if(numOfTitles == 0)
                    ReturnString +=$"{title,1}";
                else
                    ReturnString += $"\n\t\t\t\t\t      {title}";
                numOfTitles++;
            }
         
            
            return ReturnString;

        }

        public static string listAllBooks(Character character)
        {

            string ReturnString = "";
        
            int numOfStrings = 0;
            foreach (string book in character.Books!)
            {
                if (numOfStrings == 0)
                    ReturnString += $"{book,1}";
                else
                    ReturnString += $"\n\t\t\t\t\t      {book}";
                numOfStrings++;
            }

            return ReturnString;

        }



        // Funktion för att hämta swornMembers från URL:er
        static async Task<List<Character>> GetSwornMembers(HttpClient client, List<string> swornMemberUrls, String CurrentHouseName)
        {
            //List<string> swornMembers = new List<string>();
            List<Character> swornMemberCharacters = new List<Character>();

            foreach (string swornMemberUrl in swornMemberUrls)
            {
                if (!string.IsNullOrEmpty(swornMemberUrl)) // Lägg till kontroll för null och tomma URL:er
                {
                    HttpResponseMessage response = await client.GetAsync(swornMemberUrl);
                    response.EnsureSuccessStatusCode();
                    string responseData = await response.Content.ReadAsStringAsync();
                    Character swornMemberData = JsonConvert.DeserializeObject<Character>(responseData)!;
                    swornMemberCharacters.Add(swornMemberData);
                    int elementNum = swornMemberCharacters.Count();
                    swornMemberCharacters[elementNum-1].SetHouseName(CurrentHouseName);

                 
                    //if (swornMemberData != null && swornMemberData.Name != null) // Lägg till kontroll för null-swornMemberData och swornMember namn
                    //{
                    //    string swornMemberName = swornMemberData.Name;
                    //swornMembers.Add(swornMemberName);
                    //}
                }
            }
            //swornMemberCharacters.Sort();
            return swornMemberCharacters;
        }
    }

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

        public int CompareTo(Character other)
        {
            return Name.CompareTo(other.Name);
        }

        public override string ToString()
        {
            
            string ReturnString =$"{Name, -16} {HouseName, -28} ";
            /*
            int numOfStrings = 0;
            foreach(string title in Titles!)
            {
                if(numOfStrings == 0)
                    ReturnString +=$"{title,1}";
                else
                    ReturnString += $"\n\t\t\t\t\t       {title}";
                numOfStrings++;
            }

            foreach (string book in Books!)
            {
                if (numOfStrings == 0)
                    ReturnString += $"{book,1}";
                else
                    ReturnString += $"\n\t\t\t\t\t       {book}";
                numOfStrings++;
            }
            */
            return ReturnString;
        }

    }

}
