namespace DiabeteCheck.Utils
{
    public static class TriggerTerms
    {
        // Liste des termes racines à surveiller
        public static readonly List<string> Terms = new()
        {
            "Hémoglobine A1C", "Microalbumine", "Taille", "Poids",
            "Fumeur",
            "Anormal",
            "Cholestérol", "Vertige",
            "Rechute", "Réaction", "Anticorps"
        };

        // Dictionnaire des variantes (clé = terme racine, valeurs = toutes les formes possibles)
        public static readonly Dictionary<string, List<string>> TermsWithVariants =
            Terms
                .Select(t => t.ToLowerInvariant())
                .Distinct()
                .ToDictionary(term => term, term => GenerateVariants(term));

        // Génère toutes les variantes d’un mot (pluriel, genre, simplification d’accents)
        private static List<string> GenerateVariants(string term)
        {
            var variants = new List<string> { term };

            // Pluriel : ajouter "s" si le mot ne finit pas déjà par "s"
            if (!term.EndsWith("s"))
                variants.Add(term + "s");

            // Féminin ↔ Masculin
            var genreEquivalents = new Dictionary<string, string>
            {
                { "fumeur", "fumeuse" },
                { "fumeuse", "fumeur" },
                { "anormal", "anormale" },
                { "anormale", "anormal" },
                { "vertige", "vertiges" },
                { "vertiges", "vertige" }
            };

            if (genreEquivalents.TryGetValue(term, out var alt))
                variants.Add(alt);

            // Ajoute une version simplifiée sans accents (ex : réaction → reaction)
            variants.Add(RemoveAccents(term));

            // On applique aussi la version sans accents aux formes générées
            var variantsWithoutAccents = variants
                .Select(RemoveAccents)
                .ToList();

            // Fusion et dédoublonnage
            variants.AddRange(variantsWithoutAccents);

            return variants
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct()
                .ToList();
        }

        // Supprime les accents les plus courants
        private static string RemoveAccents(string input)
        {
            return input
                .Replace("é", "e")
                .Replace("è", "e")
                .Replace("ê", "e")
                .Replace("ë", "e")
                .Replace("à", "a")
                .Replace("ù", "u")
                .Replace("ô", "o")
                .Replace("î", "i")
                .Replace("ï", "i")
                .Replace("â", "a");
        }
    }
}