
using System;
using System.Net;
using System.Text;

namespace CharacterCatalogApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("завантаження даних з genshin api");

            GenshinApiService apiService = new GenshinApiService();
            CatalogBusinessService catalogService = new CatalogBusinessService();

            string rawJson = apiService.GetCharacterJson("zhongli");

            CharacterDomainModel domainChar =
                catalogService.ParseCharacter(rawJson, "zhongli");

            CharacterViewModel viewModel =
                catalogService.CreateViewModel(domainChar);

            CliDisplay cli = new CliDisplay();
            cli.ShowCharacter(viewModel);

            Console.WriteLine("\nнатисніть enter для виходу");
            Console.ReadLine();
        }
    }

    public class GenshinApiService
    {
        public string GetCharacterJson(string id)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;

                    string url =
                        "https://genshin.jmp.blue/characters/" + id;

                    return client.DownloadString(url);
                }
            }
            catch
            {
                return "{\"name\":\"zhongli\",\"vision\":\"geo\",\"weapon\":\"polearm\",\"rarity\":5,\"description\":\"consultant of the wangsheng funeral parlor.\"}";
            }
        }
    }

    public class CharacterDomainModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Element { get; set; }
        public string WeaponType { get; set; }
        public int Rarity { get; set; }
        public string Description { get; set; }
        public string CardImageUrl { get; set; }
    }

    public class CharacterViewModel
    {
        public string DisplayTitle { get; set; }
        public string SpecsLine { get; set; }
        public string Bio { get; set; }
        public string ImageLink { get; set; }
    }

    public class CatalogBusinessService
    {
        private string GetJsonValue(string json, string key)
        {
            string search = "\"" + key + "\"";

            int keyIndex = json.IndexOf(search);

            if (keyIndex < 0)
                return "";

            int colonIndex = json.IndexOf(":", keyIndex);

            if (colonIndex < 0)
                return "";

            int start = json.IndexOf("\"", colonIndex);

            if (start < 0)
                return "";

            int end = json.IndexOf("\"", start + 1);

            if (end < 0)
                return "";

            return json.Substring(start + 1, end - start - 1);
        }

        private int GetJsonNumber(string json, string key, int defaultValue)
        {
            string search = "\"" + key + "\"";

            int keyIndex = json.IndexOf(search);

            if (keyIndex < 0)
                return defaultValue;

            int colonIndex = json.IndexOf(":", keyIndex);

            if (colonIndex < 0)
                return defaultValue;

            int start = colonIndex + 1;

            while (start < json.Length && Char.IsWhiteSpace(json[start]))
                start++;

            int end = start;

            while (end < json.Length && Char.IsDigit(json[end]))
                end++;

            int result;

            if (Int32.TryParse(json.Substring(start, end - start), out result))
                return result;

            return defaultValue;
        }

        public CharacterDomainModel ParseCharacter(string json, string id)
        {
            CharacterDomainModel model = new CharacterDomainModel();

            model.Id = id;

            model.Name = GetJsonValue(json, "name");
            model.Element = GetJsonValue(json, "vision");
            model.WeaponType = GetJsonValue(json, "weapon");
            model.Rarity = GetJsonNumber(json, "rarity", 4);
            model.Description = GetJsonValue(json, "description");

            if (String.IsNullOrEmpty(model.Name))
                model.Name = "unknown";

            if (String.IsNullOrEmpty(model.Element))
                model.Element = "none";

            if (String.IsNullOrEmpty(model.WeaponType))
                model.WeaponType = "none";

            if (String.IsNullOrEmpty(model.Description))
                model.Description = "no description";

            model.CardImageUrl =
                "https://genshin.jmp.blue/characters/" + id + "/card";

            return model;
        }

        public CharacterViewModel CreateViewModel(CharacterDomainModel domain)
        {
            CharacterViewModel vm = new CharacterViewModel();

            vm.DisplayTitle =
                domain.Name.ToUpper() + " (" + domain.Rarity + "★)";

            vm.SpecsLine =
                "стихія: " + domain.Element +
                " | зброя: " + domain.WeaponType;

            vm.Bio = domain.Description;
            vm.ImageLink = domain.CardImageUrl;

            return vm;
        }
    }

    public class CliDisplay
    {
        public void ShowCharacter(CharacterViewModel vm)
        {
            Console.WriteLine("\n=========================================");
            Console.WriteLine(" " + vm.DisplayTitle);
            Console.WriteLine(" " + vm.SpecsLine);
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine(" опис: " + vm.Bio);
            Console.WriteLine(" арт:  " + vm.ImageLink);
            Console.WriteLine("=========================================");
        }
    }
}


