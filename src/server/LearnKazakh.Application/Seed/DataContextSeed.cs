using LearnKazakh.Domain.Entities;
using LearnKazakh.Infrastructure.Authentication;
using LearnKazakh.Persistence;
using LearnKazakh.Shared.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LearnKazakh.Application.Seed;

public static class DataContextSeed
{
    private static readonly PasswordService _passwordService;
    private static Func<string>? _getAdminPassword;

    static DataContextSeed()
    {
        _passwordService = new PasswordService();
    }

    public static async Task SeedAsync(this IServiceCollection _, Func<string>? getAdminPassword, LearnKazakhContext ctx)
    {
        _getAdminPassword = getAdminPassword;

        await SeedIfEmpty(ctx, ctx.Users, GetAdmin());
        await SeedIfEmpty(ctx, ctx.Categories, GetCategories());
        await SeedIfEmpty(ctx, ctx.Sections, GetSections());
        await SeedIfEmpty(ctx, ctx.Vocabularies, GetVocabularies());
        await SeedIfEmpty(ctx, ctx.VocabularyExamples, GetVocabularyExamples());
    }

    public static async Task SeedIfEmpty<T>(LearnKazakhContext ctx, DbSet<T> dbSet, IEnumerable<T> data) where T : class
    {
        if (!await dbSet.AnyAsync())
        {
            await dbSet.AddRangeAsync(data);
            await ctx.SaveChangesAsync();
        }
    }

    private static IEnumerable<User> GetAdmin()
    {
        string password = _getAdminPassword?.Invoke() ?? "YourDefaultPassword123!";
        (string hash, string salt) = _passwordService.HashPassword(password);

        User user = new()
        {
            Email = "dev.ahmadov.mahammad@gmail.com",
            FirstName = "Mahammad",
            LastName = "Ahmadov",
            PasswordHash = hash,
            PasswordSalt = salt,
            EmailVerified = true,
        };

        new List<string> { "Admin", "Instructor" }.ForEach(role =>
        {
            user.UserRoles.Add(new Domain.Entities.UserRole
            {
                Role = new Role
                {
                    Name = role,
                }
            });
        });

        return [user];
    }

    private static readonly Guid AlphabetCategoryId = new("941c0ef8-e6aa-4bb0-ac6b-f9941d6151f2");
    private static readonly Guid NumbersCategoryId = new("d8d87b51-781f-4b80-b549-b3719c770436");
    private static readonly Guid GrammarCategoryId = new("33941de7-7b48-479d-8990-53f3c15f9847");
    private static readonly Guid DailyLifeCategoryId = new("4f5bf5e2-c4f7-40bf-b6a4-1f20cd0909ea");

    public static List<Category> GetCategories() =>
    [
        new() { Id = AlphabetCategoryId, Name = "Alphabet", Description = "Learn the Kazakh alphabet with pronunciation and examples", Icon = GetAlphabetIcon(), CreatedBy = "System" },
        new() { Id = NumbersCategoryId, Name = "Numbers", Description = "Learn Kazakh numbers and counting", Icon = GetNumbersIcon(), CreatedBy = "System" },
        new() { Id = GrammarCategoryId, Name = "Grammar", Description = "Essential Kazakh grammar rules and structures", Icon = GetGrammarIcon(), CreatedBy = "System" },
        new() { Id = DailyLifeCategoryId, Name = "Daily Life", Description = "Practical expressions for everyday situations", Icon = GetDailyLifeIcon(), CreatedBy = "System" },
    ];

    public static List<Section> GetSections() =>
    [
        .. CreateSections(AlphabetCategoryId, [
            "А-Ж Letters", "З-П Letters", "Р-Ш Letters", "Щ-Я Letters"
        ]),

        .. CreateSections(NumbersCategoryId, [
            "Basic Numbers", "Tens & Hundreds", "Large Numbers", "Ordinals"
        ]),

        .. CreateSections(GrammarCategoryId, [
            "Pronouns", "Verb 'To Be'", "Present Tense", "Past Tense",
            "Future Tense", "Nominative", "Genitive", "Dative",
            "Accusative", "Locative", "Ablative", "Instrumental",
            "Plurals", "Possessives", "Comparatives", "Questions",
            "Negation", "Modal Verbs", "Participles", "Conditionals"
        ]),

        .. CreateSections(DailyLifeCategoryId, [
            "Time & Days", "Greetings", "Family Terms", "Weather",
            "Directions", "Shopping", "Restaurant", "Transportation"
        ]),
    ];

    private static List<Section> CreateSections(Guid categoryId, string[] titles)
    {
        return [.. titles.Select((title, index) => new Section
        {
            Id = Guid.NewGuid(),
            Title = title,
            Order = index + 1,
            CategoryId = categoryId,
            CreatedBy = "System"
        })];
    }

    private static readonly Dictionary<int, Guid> VocabularyGuids = new()
    {
        { 1, new("3fcb4b86-96ac-4ff8-bc3a-b7a6b6ac2085") },
        { 2, new("5e6cd08d-895a-430b-aa8e-4c5f50f6e52d") },
        { 3, new("32f85339-d394-416f-a766-cb6b3f85f9e4") },
        { 4, new("aa1bc27d-180d-46bb-b530-ad7c7f5f2024") },
        { 5, new("68abc4e7-5cb3-41ec-9d3b-c53c5ea01aee") },
        { 6, new("95ac44d9-a628-49b9-a819-466617965110") },
        { 7, new("130bb2be-4566-4398-8849-505127028988") },
        { 8, new("e73aafdc-22bb-43c2-823e-01a0e57d7672") },
        { 9, new("1244fec9-2705-47eb-889d-9419fa3b600a") },
        { 10, new("1edd474a-ef1b-4262-8b7b-10bc60661091") },
        { 11, new("4f6b97d5-b8c7-448c-b9e0-a9259e1540cd") },
        { 12, new("157bacc4-6685-4d0e-9f67-d47e9b7b195c") },
        { 13, new("304ea3c1-ed7b-4daa-b241-01af1c5b5d5b") },
        { 14, new("dbe4e905-a434-41b2-899e-79ace0ca7551") },
        { 15, new("933f502f-d871-48e8-b25e-b820a041c768") },
        { 16, new("ce20f9be-8444-4451-a2fb-f5dc024a4085") },
        { 17, new("4bfd4426-5234-4d1c-9fe4-546fd7b4bbe8") },
        { 18, new("22d2f9dd-fde7-4b36-b252-2204cae5a4ba") },
        { 19, new("2908aee0-8742-4a30-8a99-7e5773424a96") },
        { 20, new("50b68651-e4da-483e-9014-fb0b8ab07b9b") },
        { 21, new("d42d1c5b-d491-4e33-b18a-5c63b56e5642") },
        { 22, new("40861a6c-31ce-4a35-bdfc-1939d923e0b2") },
        { 23, new("565d348b-03a0-486c-a3b2-f1b69960880e") },
        { 24, new("8ab89750-8d8d-4a89-9642-3abad596c0f0") },
        { 25, new("65c44d0f-5adf-484b-9a11-57bc2e68019f") },
        { 26, new("c93f7fc9-6346-451a-9487-e9dfc5cd0ee0") },
        { 27, new("ecd8ae75-1de4-435b-8b4b-9effd0adc42a") },
        { 28, new("0ce5c33e-a590-4199-a905-973aa8acef49") },
        { 29, new("d037f2c1-42ce-4409-a795-428a8efd4fea") },
        { 30, new("d5604f27-4169-416e-8337-0a9fa23822af") },
        { 31, new("7d4e414e-3d87-4140-9a30-fad8adb8a87f") },
        { 32, new("ae73daf0-b833-4f82-b68a-deca850e899f") },
        { 33, new("604830df-c5ef-4a29-9557-9a024fad9813") },
        { 34, new("89989de2-5be2-40ec-bd29-cabd10227685") },
        { 35, new("0a87a55c-ab8e-4e7e-b3cd-de7939bb2396") },
        { 36, new("4a315cd1-e24a-4f92-b0fd-f9d94fc0b625") },
        { 37, new("368dc010-2311-4537-82b7-06b0071d6136") },
        { 38, new("ef8f3c2b-fe7f-4dee-b307-69beec7d4792") },
        { 39, new("1e6c8dca-8182-4ded-9a9b-9164f71376a7") },
        { 40, new("1b0b1934-209e-462e-8281-cf3e84cce320") },
        { 41, new("7cd68193-3426-46a1-849c-de13f7585696") },
        { 42, new("bdaf55fd-0634-41c8-8c2d-c061fde2f24b") },
        { 43, new("2e49b13f-7779-49f0-b529-36146b4bacb7") },
        { 44, new("262bb6d0-4df1-4953-b3b0-8fc2c6b55152") },
        { 45, new("79b9bb6e-45ac-4418-87f9-b29da6b0c67c") },
        { 46, new("f0e6fcef-478f-435f-8f7e-ba4091f285d7") },
        { 47, new("7123f097-24ce-4d07-bae5-d83de5f843d1") },
        { 48, new("3d504b85-6a74-4912-9141-8e55a180fe15") },
        { 49, new("46b5c8ac-a5f4-4bb7-bc37-8baaac5764d5") },
        { 50, new("ff0c0f37-3367-475e-bfdf-96458782a45d") }
    };

    public static List<Vocabulary> GetVocabularies() =>
    [
        // Greetings
        new() { Id = VocabularyGuids[1], WordKazakh = "Сәлем", WordAzerbaijani = "Salam", Pronounciation = "sah-lem", Type = VocabularyType.Greetings, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[2], WordKazakh = "Сау болыңыз", WordAzerbaijani = "Sağ olun", Pronounciation = "saw bo-luh-nuhz", Type = VocabularyType.Greetings, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[3], WordKazakh = "Рахмет", WordAzerbaijani = "Təşəkkür", Pronounciation = "rah-met", Type = VocabularyType.Greetings, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[4], WordKazakh = "Кешіріңіз", WordAzerbaijani = "Bağışlayın", Pronounciation = "ke-shee-ree-neez", Type = VocabularyType.Greetings, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[5], WordKazakh = "Қайырлы таң", WordAzerbaijani = "Sabahınız xeyir", Pronounciation = "qai-uh-ruh-luh tang", Type = VocabularyType.Greetings, CreatedBy = "System", CreatedAt = DateTime.UtcNow },

        // Numbers
        new() { Id = VocabularyGuids[6], WordKazakh = "бір", WordAzerbaijani = "bir", Pronounciation = "beer", Type = VocabularyType.Numbers, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[7], WordKazakh = "екі", WordAzerbaijani = "iki", Pronounciation = "ye-kee", Type = VocabularyType.Numbers, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[8], WordKazakh = "үш", WordAzerbaijani = "üç", Pronounciation = "oosh", Type = VocabularyType.Numbers, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[9], WordKazakh = "төрт", WordAzerbaijani = "dörd", Pronounciation = "tort", Type = VocabularyType.Numbers, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[10], WordKazakh = "бес", WordAzerbaijani = "beş", Pronounciation = "bes", Type = VocabularyType.Numbers, CreatedBy = "System", CreatedAt = DateTime.UtcNow },

        // Family Members
        new() { Id = VocabularyGuids[11], WordKazakh = "ана", WordAzerbaijani = "ana", Pronounciation = "ah-nah", Type = VocabularyType.FamilyMembers, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[12], WordKazakh = "әке", WordAzerbaijani = "ata", Pronounciation = "ah-ke", Type = VocabularyType.FamilyMembers, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[13], WordKazakh = "бала", WordAzerbaijani = "uşaq", Pronounciation = "bah-lah", Type = VocabularyType.FamilyMembers, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[14], WordKazakh = "ағайынды", WordAzerbaijani = "qardaş", Pronounciation = "ah-gah-uhn-duh", Type = VocabularyType.FamilyMembers, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[15], WordKazakh = "әпке", WordAzerbaijani = "bacı", Pronounciation = "ahp-ke", Type = VocabularyType.FamilyMembers, CreatedBy = "System", CreatedAt = DateTime.UtcNow },

        // Colors
        new() { Id = VocabularyGuids[16], WordKazakh = "қызыл", WordAzerbaijani = "qırmızı", Pronounciation = "quh-zuhl", Type = VocabularyType.Colors, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[17], WordKazakh = "көк", WordAzerbaijani = "mavi", Pronounciation = "kook", Type = VocabularyType.Colors, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[18], WordKazakh = "жасыл", WordAzerbaijani = "yaşıl", Pronounciation = "zhah-suhl", Type = VocabularyType.Colors, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[19], WordKazakh = "сары", WordAzerbaijani = "sarı", Pronounciation = "sah-ruh", Type = VocabularyType.Colors, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[20], WordKazakh = "ақ", WordAzerbaijani = "ağ", Pronounciation = "ahq", Type = VocabularyType.Colors, CreatedBy = "System", CreatedAt = DateTime.UtcNow },

        // Animals
        new() { Id = VocabularyGuids[21], WordKazakh = "мысық", WordAzerbaijani = "pişik", Pronounciation = "muh-suhq", Type = VocabularyType.Animals, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[22], WordKazakh = "ит", WordAzerbaijani = "it", Pronounciation = "eet", Type = VocabularyType.Animals, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[23], WordKazakh = "жылқы", WordAzerbaijani = "at", Pronounciation = "zhuh-lquh", Type = VocabularyType.Animals, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[24], WordKazakh = "сиыр", WordAzerbaijani = "inək", Pronounciation = "see-uhr", Type = VocabularyType.Animals, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[25], WordKazakh = "қой", WordAzerbaijani = "qoyun", Pronounciation = "qoi", Type = VocabularyType.Animals, CreatedBy = "System", CreatedAt = DateTime.UtcNow },

        // Food and Drinks
        new() { Id = VocabularyGuids[26], WordKazakh = "нан", WordAzerbaijani = "çörək", Pronounciation = "nahn", Type = VocabularyType.FoodAndDrinks, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[27], WordKazakh = "су", WordAzerbaijani = "su", Pronounciation = "soo", Type = VocabularyType.FoodAndDrinks, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[28], WordKazakh = "шай", WordAzerbaijani = "çay", Pronounciation = "shai", Type = VocabularyType.FoodAndDrinks, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[29], WordKazakh = "ет", WordAzerbaijani = "ət", Pronounciation = "yet", Type = VocabularyType.FoodAndDrinks, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[30], WordKazakh = "сүт", WordAzerbaijani = "süd", Pronounciation = "soot", Type = VocabularyType.FoodAndDrinks, CreatedBy = "System", CreatedAt = DateTime.UtcNow },

        // Days of Week
        new() { Id = VocabularyGuids[31], WordKazakh = "дүйсенбі", WordAzerbaijani = "bazar ertəsi", Pronounciation = "doo-ee-sen-bee", Type = VocabularyType.DaysOfWeek, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[32], WordKazakh = "сейсенбі", WordAzerbaijani = "çərşənbə axşamı", Pronounciation = "say-sen-bee", Type = VocabularyType.DaysOfWeek, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[33], WordKazakh = "сәрсенбі", WordAzerbaijani = "çərşənbə", Pronounciation = "sahr-sen-bee", Type = VocabularyType.DaysOfWeek, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[34], WordKazakh = "бейсенбі", WordAzerbaijani = "cümə axşamı", Pronounciation = "bay-sen-bee", Type = VocabularyType.DaysOfWeek, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[35], WordKazakh = "жұма", WordAzerbaijani = "cümə", Pronounciation = "zhoo-mah", Type = VocabularyType.DaysOfWeek, CreatedBy = "System", CreatedAt = DateTime.UtcNow },

        // Body Parts
        new() { Id = VocabularyGuids[36], WordKazakh = "бас", WordAzerbaijani = "baş", Pronounciation = "bahs", Type = VocabularyType.BodyParts, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[37], WordKazakh = "көз", WordAzerbaijani = "göz", Pronounciation = "kooz", Type = VocabularyType.BodyParts, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[38], WordKazakh = "қол", WordAzerbaijani = "əl", Pronounciation = "qol", Type = VocabularyType.BodyParts, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[39], WordKazakh = "аяқ", WordAzerbaijani = "ayaq", Pronounciation = "ah-yahq", Type = VocabularyType.BodyParts, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[40], WordKazakh = "ауыз", WordAzerbaijani = "ağız", Pronounciation = "ah-oohz", Type = VocabularyType.BodyParts, CreatedBy = "System", CreatedAt = DateTime.UtcNow },

        // Weather
        new() { Id = VocabularyGuids[41], WordKazakh = "күн", WordAzerbaijani = "günəş", Pronounciation = "koon", Type = VocabularyType.Weather, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[42], WordKazakh = "жаңбыр", WordAzerbaijani = "yağış", Pronounciation = "zhahn-buhr", Type = VocabularyType.Weather, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[43], WordKazakh = "қар", WordAzerbaijani = "qar", Pronounciation = "qahr", Type = VocabularyType.Weather, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[44], WordKazakh = "жел", WordAzerbaijani = "külək", Pronounciation = "zhel", Type = VocabularyType.Weather, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[45], WordKazakh = "бұлт", WordAzerbaijani = "bulud", Pronounciation = "boolt", Type = VocabularyType.Weather, CreatedBy = "System", CreatedAt = DateTime.UtcNow },

        // Places
        new() { Id = VocabularyGuids[46], WordKazakh = "үй", WordAzerbaijani = "ev", Pronounciation = "ooy", Type = VocabularyType.Places, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[47], WordKazakh = "мектеп", WordAzerbaijani = "məktəb", Pronounciation = "mek-tep", Type = VocabularyType.Places, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[48], WordKazakh = "дүкен", WordAzerbaijani = "mağaza", Pronounciation = "doo-ken", Type = VocabularyType.Places, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[49], WordKazakh = "аурухана", WordAzerbaijani = "xəstəxana", Pronounciation = "ah-oo-roo-hah-nah", Type = VocabularyType.Places, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = VocabularyGuids[50], WordKazakh = "банк", WordAzerbaijani = "bank", Pronounciation = "bahnk", Type = VocabularyType.Places, CreatedBy = "System", CreatedAt = DateTime.UtcNow }
    ];

    public static List<VocabularyExample> GetVocabularyExamples() =>
    [
        // Examples for Greetings
        new() { Id = Guid.NewGuid(), VocabularyId = VocabularyGuids[1], SentenceKazakh = "Сәлем, қалың қалай?", SentenceTranslation = "Salam, necəsən?", CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = Guid.NewGuid(), VocabularyId = VocabularyGuids[1], SentenceKazakh = "Сәлем алейкум!", SentenceTranslation = "Əssəlamü aleyküm!", CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = Guid.NewGuid(), VocabularyId = VocabularyGuids[3], SentenceKazakh = "Көмегіңіз үшін рахмет.", SentenceTranslation = "Köməyiniz üçün təşəkkür.", CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = Guid.NewGuid(), VocabularyId = VocabularyGuids[3], SentenceKazakh = "Рахмет, өте жақсы!", SentenceTranslation = "Təşəkkür, çox yaxşı!", CreatedBy = "System", CreatedAt = DateTime.UtcNow },

        // Examples for Numbers
        new() { Id = Guid.NewGuid(), VocabularyId = VocabularyGuids[6], SentenceKazakh = "Менде бір кітап бар.", SentenceTranslation = "Məndə bir kitab var.", CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = Guid.NewGuid(), VocabularyId = VocabularyGuids[7], SentenceKazakh = "Екі балам бар.", SentenceTranslation = "İki uşağım var.", CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = Guid.NewGuid(), VocabularyId = VocabularyGuids[8], SentenceKazakh = "Үш сағат күттім.", SentenceTranslation = "Üç saat gözlədim.", CreatedBy = "System", CreatedAt = DateTime.UtcNow },

        // Examples for Family
        new() { Id = Guid.NewGuid(), VocabularyId = VocabularyGuids[11], SentenceKazakh = "Менің анам мұғалім.", SentenceTranslation = "Mənim anam müəllimdir.", CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = Guid.NewGuid(), VocabularyId = VocabularyGuids[12], SentenceKazakh = "Әкем жұмыста.", SentenceTranslation = "Atam işdədir.", CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = Guid.NewGuid(), VocabularyId = VocabularyGuids[13], SentenceKazakh = "Бала мектепке барады.", SentenceTranslation = "Uşaq məktəbə gedir.", CreatedBy = "System", CreatedAt = DateTime.UtcNow },

        // Examples for Colors
        new() { Id = Guid.NewGuid(), VocabularyId = VocabularyGuids[16], SentenceKazakh = "Қызыл алма.", SentenceTranslation = "Qırmızı alma.", CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = Guid.NewGuid(), VocabularyId = VocabularyGuids[17], SentenceKazakh = "Аспан көк түсті.", SentenceTranslation = "Səma mavi rəngdədir.", CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = Guid.NewGuid(), VocabularyId = VocabularyGuids[18], SentenceKazakh = "Жасыл шөп.", SentenceTranslation = "Yaşıl ot.", CreatedBy = "System", CreatedAt = DateTime.UtcNow },

        // Examples for Animals
        new() { Id = Guid.NewGuid(), VocabularyId = VocabularyGuids[21], SentenceKazakh = "Мысық сүт ішеді.", SentenceTranslation = "Pişik süd içir.", CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = Guid.NewGuid(), VocabularyId = VocabularyGuids[22], SentenceKazakh = "Ит үйді қорғайды.", SentenceTranslation = "İt evi qoruyur.", CreatedBy = "System", CreatedAt = DateTime.UtcNow },

        // Examples for Food
        new() { Id = Guid.NewGuid(), VocabularyId = VocabularyGuids[26], SentenceKazakh = "Мен нан жеймін.", SentenceTranslation = "Mən çörək yeyirəm.", CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = Guid.NewGuid(), VocabularyId = VocabularyGuids[27], SentenceKazakh = "Су ішкім келеді.", SentenceTranslation = "Su içmək istəyirəm.", CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = Guid.NewGuid(), VocabularyId = VocabularyGuids[28], SentenceKazakh = "Шай қайнатып жатырмын.", SentenceTranslation = "Çay qaynadıram.", CreatedBy = "System", CreatedAt = DateTime.UtcNow },

        // Examples for Body Parts
        new() { Id = Guid.NewGuid(), VocabularyId = VocabularyGuids[36], SentenceKazakh = "Менің басым ауырады.", SentenceTranslation = "Mənim başım ağrıyır.", CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = Guid.NewGuid(), VocabularyId = VocabularyGuids[37], SentenceKazakh = "Көзің қандай әдемі!", SentenceTranslation = "Gözlərin nə qədər gözəl!", CreatedBy = "System", CreatedAt = DateTime.UtcNow },

        // Examples for Weather
        new() { Id = Guid.NewGuid(), VocabularyId = VocabularyGuids[41], SentenceKazakh = "Бүгін күн ашық.", SentenceTranslation = "Bu gün günəşli hava var.", CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = Guid.NewGuid(), VocabularyId = VocabularyGuids[42], SentenceKazakh = "Жаңбыр жауып тұр.", SentenceTranslation = "Yağış yağır.", CreatedBy = "System", CreatedAt = DateTime.UtcNow },

        // Examples for Places
        new() { Id = Guid.NewGuid(), VocabularyId = VocabularyGuids[46], SentenceKazakh = "Мен үйде отырмын.", SentenceTranslation = "Mən evdə otururam.", CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = Guid.NewGuid(), VocabularyId = VocabularyGuids[47], SentenceKazakh = "Балалар мектепте.", SentenceTranslation = "Uşaqlar məktəbdədir.", CreatedBy = "System", CreatedAt = DateTime.UtcNow },
        new() { Id = Guid.NewGuid(), VocabularyId = VocabularyGuids[48], SentenceKazakh = "Дүкеннен нан сатып алдым.", SentenceTranslation = "Mağazadan çörək aldım.", CreatedBy = "System", CreatedAt = DateTime.UtcNow }
    ];

    // helpers
    private static string GetAlphabetIcon() => "<svg xmlns='http://www.w3.org/2000/svg' width='20' height='20' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'>    <path d='M15 11h4.5a1 1 0 0 1 0 5h-4a.5.5 0 0 1-.5-.5v-9a.5.5 0 0 1 .5-.5h3a1 1 0 0 1 0 5'/><path d='m2 16 4.039-9.69a.5.5 0 0 1 .923 0L11 16'/><path d='M3.304 13h6.392'/></svg>";
    private static string GetNumbersIcon() => "<svg xmlns='http://www.w3.org/2000/svg' width='20' height='20' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'>    <path d='m3 16 4 4 4-4'/><path d='M7 20V4'/><rect x='15' y='4' width='4' height='6' ry='2'/><path d='M17 20v-6h-2'/><path d='M15 20h4'/></svg>";
    private static string GetGrammarIcon() => "<svg xmlns='http://www.w3.org/2000/svg' width='20' height='20' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'><path d='M19.414 14.414C21 12.828 22 11.5 22 9.5a5.5 5.5 0 0 0-9.591-3.676.6.6 0 0 1-.818.001A5.5 5.5 0 0 0 2 9.5c0 2.3 1.5 4 3 5.5l5.535 5.362a2 2 0 0 0 2.879.052 2.12 2.12 0 0 0-.004-3 2.124 2.124 0 1 0 3-3 2.124 2.124 0 0 0 3.004 0 2 2 0 0 0 0-2.828l-1.881-1.882a2.41 2.41 0 0 0-3.409 0l-1.71 1.71a2 2 0 0 1-2.828 0 2 2 0 0 1 0-2.828l2.823-2.762'/></svg>";
    private static string GetDailyLifeIcon() => "<svg xmlns='http://www.w3.org/2000/svg' width='20' height='20' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'><path d='M12 7v14'/><path d='M16 12h2'/><path d='M16 8h2'/><path d='M3 18a1 1 0 0 1-1-1V4a1 1 0 0 1 1-1h5a4 4 0 0 1 4 4 4 4 0 0 1 4-4h5a1 1 0 0 1 1 1v13a1 1 0 0 1-1 1h-6a3 3 0 0 0-3 3 3 3 0 0 0-3-3z'/><path d='M6 12h2'/><path d='M6 8h2'/></svg>";
}

