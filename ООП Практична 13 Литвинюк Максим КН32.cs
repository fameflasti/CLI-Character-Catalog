
using System;
using System.Collections.Generic;
using System.Linq;

namespace MainApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var renderer = new CliLayer.ConsoleRenderer();
            var cli = new CliLayer.CliFacade(renderer);

            cli.UseStrategy(new CliLayer.InformationStrategy());
            cli.UseStrategy(new CliLayer.ManagementStrategy());

            cli.StartInteractiveLoop();
        }
    }
}

namespace CliLayer
{

    public class Args
    {
        public string CommandName { get; }
        public string PrimaryTarget { get; }

        private readonly Dictionary<string, string> _options =
            new Dictionary<string, string>();

        public Args(string rawInput)
        {
            if (string.IsNullOrWhiteSpace(rawInput))
                return;

            string[] tokens = rawInput.Split(
                new char[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries);

            CommandName = tokens[0].ToLower();

            int optionStartIndex = tokens.Length;

            for (int i = 1; i < tokens.Length; i++)
            {
                if (tokens[i].StartsWith("--"))
                {
                    optionStartIndex = i;
                    break;
                }
            }

            if (optionStartIndex > 1)
            {
                PrimaryTarget = string.Join(
                    " ",
                    tokens.Skip(1).Take(optionStartIndex - 1));
            }

            for (int i = optionStartIndex; i < tokens.Length; i += 2)
            {
                if (i + 1 < tokens.Length)
                {
                    _options[tokens[i].ToLower()] = tokens[i + 1];
                }
            }
        }

        public string GetOption(string key)
        {
            return _options.ContainsKey(key.ToLower())
                ? _options[key.ToLower()]
                : "";
        }
    }

    public interface IRenderer
    {
        void RenderText(string text);
        void RenderSuccess(string message);
        void RenderWarning(string warning);

        void RenderProfileCard(
            string name,
            string role,
            string bio,
            string graphicsUrl);
    }

    public class ConsoleRenderer : IRenderer
    {
        public void RenderText(string text)
        {
            Console.WriteLine(text);
        }

        public void RenderSuccess(string message)
        {
            Console.WriteLine(" [SUCCESS] " + message);
        }

        public void RenderWarning(string warning)
        {
            Console.WriteLine(" [WARNING] " + warning);
        }

        public void RenderProfileCard(
            string name,
            string role,
            string bio,
            string graphicsUrl)
        {
            Console.WriteLine("\n==================================================");
            Console.WriteLine(" CHARACTER PROFILE: " + name.ToUpper());
            Console.WriteLine("==================================================");
            Console.WriteLine(" Class / Role : " + role);
            Console.WriteLine(" Biography    : " + bio);
            Console.WriteLine(" Graphics Link: " + graphicsUrl);
            Console.WriteLine("==================================================\n");
        }
    }
    
    public interface ICommandStrategy
    {
        ICollection<string> GetSupportedCommands();

        void Execute(
            string command,
            Args args,
            IRenderer renderer,
            List<DomainLayer.CharacterModel> database);
    }

    public class InformationStrategy : ICommandStrategy
    {
        public ICollection<string> GetSupportedCommands()
        {
            return new[] { "ls", "info" };
        }

        public void Execute(
            string command,
            Args args,
            IRenderer renderer,
            List<DomainLayer.CharacterModel> database)
        {
            if (command == "ls")
            {
                renderer.RenderText(
                    "\n--- Catalog Characters Registered ---");

                if (database.Count == 0)
                {
                    renderer.RenderWarning(
                        "The local database is empty.");
                }

                foreach (var character in database)
                {
                    renderer.RenderText(
                        "-> " + character.Name +
                        " (" + character.Role + ")");
                }
            }
            else if (command == "info")
            {
                string target = args.PrimaryTarget;

                if (string.IsNullOrEmpty(target))
                {
                    renderer.RenderWarning(
                        "Usage syntax error. Use: info <character_name>");

                    return;
                }

                var character = database.FirstOrDefault(
                    c => c.Name.ToLower() == target.ToLower());

                if (character == null)
                {
                    renderer.RenderWarning(
                        "Character '" + target + "' does not exist.");

                    return;
                }

                renderer.RenderProfileCard(
                    character.Name,
                    character.Role,
                    character.Bio,
                    character.GraphicsUrl);
            }
        }
    }

    public class ManagementStrategy : ICommandStrategy
    {
        public ICollection<string> GetSupportedCommands()
        {
            return new[] { "create", "save" };
        }

        public void Execute(
            string command,
            Args args,
            IRenderer renderer,
            List<DomainLayer.CharacterModel> database)
        {
            if (command == "create")
            {
                renderer.RenderText(
                    "\n--- Interactive Dialogue Wizard Creator ---");

                Console.Write("Enter Character Name: ");
                string name = Console.ReadLine() ?? "";

                Console.Write("Enter Class/Role Context: ");
                string role = Console.ReadLine() ?? "";

                Console.Write("Enter Description / Short Bio: ");
                string bio = Console.ReadLine() ?? "";

                Console.Write("Enter Asset URL for graphics illustration: ");
                string graphic = Console.ReadLine() ?? "";

                var newChar = new DomainLayer.CharacterModel(
                    name,
                    role,
                    bio,
                    graphic);

                database.Add(newChar);

                renderer.RenderSuccess(
                    "Character entities updated. '" +
                    name + "' initialized successfully.");
            }
            else if (command == "save")
            {
                renderer.RenderSuccess(
                    "All data serialization structures persistent storage emulation successful.");
            }
        }
    }

    public class CliFacade
    {
        private readonly IRenderer _renderer;

        private readonly List<ICommandStrategy> _strategies =
            new List<ICommandStrategy>();

        private readonly List<DomainLayer.CharacterModel> _localDatabase =
            new List<DomainLayer.CharacterModel>();

        public CliFacade(IRenderer renderer)
        {
            _renderer = renderer;

            _localDatabase.Add(
                new DomainLayer.CharacterModel(
                    "Zhongli",
                    "Geo Polearm",
                    "Consultant of Wangsheng Funeral Parlor",
                    ""));

            _localDatabase.Add(
                new DomainLayer.CharacterModel(
                    "Kamisato Ayaka",
                    "Cryo Sword",
                    "Princess of the Kamisato Clan",
                    ""));
        }

        public void UseStrategy(ICommandStrategy strategy)
        {
            _strategies.Add(strategy);
        }

        public void ExecCommand(string rawInput)
        {
            var parsedArgs = new Args(rawInput);

            if (string.IsNullOrEmpty(parsedArgs.CommandName))
                return;

            var executingStrategy = _strategies.FirstOrDefault(
                s => s.GetSupportedCommands().Contains(
                    parsedArgs.CommandName));

            if (executingStrategy != null)
            {
                executingStrategy.Execute(
                    parsedArgs.CommandName,
                    parsedArgs,
                    _renderer,
                    _localDatabase);
            }
            else
            {
                _renderer.RenderWarning(
                    "Command execution failed. Unknown terminal operator instruction: " +
                    parsedArgs.CommandName);
            }
        }

        public void StartInteractiveLoop()
        {
            _renderer.RenderText(
                "==================================================");

            _renderer.RenderText(
                "     UNIFIED CUSTOM INTERNAL CLI WORKSTATION      ");

            _renderer.RenderText(
                "==================================================");

            _renderer.RenderText(
                " Controls catalog UI: ls | info | create | save | exit\n");

            while (true)
            {
                Console.Write("app_shell> ");

                string rawInput = Console.ReadLine()?.Trim() ?? "";

                if (string.IsNullOrEmpty(rawInput))
                    continue;

                if (rawInput.ToLower() == "exit")
                    break;

                ExecCommand(rawInput);
            }

            _renderer.RenderText(
                "Terminal environment teardown successful.");
        }
    }
}

namespace DomainLayer
{
    public class CharacterModel
    {
        public string Name { get; }
        public string Role { get; }
        public string Bio { get; }
        public string GraphicsUrl { get; }

        public CharacterModel(
            string name,
            string role,
            string bio,
            string graphicsUrl)
        {
            Name = name;
            Role = role;
            Bio = bio;
            GraphicsUrl = graphicsUrl;
        }
    }
}
