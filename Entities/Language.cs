using System;
using System.Collections.Generic;
using System.Linq;

namespace SteamAccountCreator.Entities
{
    public sealed class Language : IEquatable<Language>
    {
        static Dictionary<string, Language> languages = new Dictionary<string, Language>
        {
            { Dutch.Code, Dutch },
            { English.Code, English },
            { French.Code, French },
            { German.Code, German },
            { Greek.Code, Greek },
            { Italian.Code, Italian },
            { Portuguese.Code, Portuguese },
            { Romanian.Code, Romanian },
            { Russian.Code, Russian },
            { Serbocroation.Code, Serbocroation },
            { Spanish.Code, Spanish }
        };

        public string Code { get; }

        public string Name { get; }

        Language(string code, string name)
        {
            Code = code;
            Name = name;
        }

        public static Language Dutch => new Language("DUT", PutSpacesInName(nameof(Dutch)));
        public static Language English => new Language("ENG", PutSpacesInName(nameof(English)));
        public static Language French => new Language("FRE", PutSpacesInName(nameof(French)));
        public static Language German => new Language("GER", PutSpacesInName(nameof(German)));
        public static Language Greek => new Language("GRE", PutSpacesInName(nameof(Greek)));
        public static Language Italian => new Language("ITA", PutSpacesInName(nameof(Italian)));
        public static Language Portuguese => new Language("POR", PutSpacesInName(nameof(Portuguese)));
        public static Language Romanian => new Language("RMN", PutSpacesInName(nameof(Romanian)));
        public static Language Russian => new Language("RUS", PutSpacesInName(nameof(Russian)));
        public static Language Serbocroation => new Language("SER", PutSpacesInName(nameof(Serbocroation)));
        public static Language Spanish => new Language("SPA", PutSpacesInName(nameof(Spanish)));

        public static IEnumerable<Language> Values => languages.Values;

        public override string ToString() => Name;

        static string PutSpacesInName(string name)
        {
            string newName = name.Substring(0, 1);

            for (int i = 1; i < name.Length; i++)
            {
                if (name[i] >= 'A' && name[i] <= 'Z')
                {
                    newName += $" {name[i]}";
                }
                else
                {
                    newName += name[i];
                }
            }

            return newName;
        }
        
        public bool Equals(Language other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Code, other.Code) &&
                   string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((Language)obj);
        }
        
        public static bool operator == (Language first, Language second)
        {
            if (first is null)
            {
                return (second is null);
            }

            return first.Equals(second);
        }

        public static bool operator != (Language first, Language second)
        {
            return !(first == second);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Code != null ? Code.GetHashCode() : 0) * 397) ^
                       (Name != null ? Name.GetHashCode() : 0);
            }
        }
    }
}
