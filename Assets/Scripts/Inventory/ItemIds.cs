using System.Collections.Generic;

public static class ItemIds {
    public const string InventoryArtefact = "InventoryArtefact"; // без "item_"
    public const string Gun = "Gun"; 
    public const string HarmonicRow = "HarmonicRow";
    public const string SonoceramicShard = "SonoceramicShard";
    public const string SonusGuideTube = "SonusGuideTube";
    public const string ReceiptWhisperer = "ReceiptWhisperer";
    public const string WaxStoppers = "WaxStoppers";
    public const string MaintScrollHum = "MaintScrollHum";
    public const string VentFiddle = "VentFiddle";
    public const string EarPressureReports = "EarPressureReports";
    public const string TestCube = "TestCube";
    public const string Mop = "Mop";
    public const string CubesNote = "CubesNote";
    public const string JapaneseSweets = "JapaneseSweets";
    public const string RatkoThings = "RatkoThings";

    public static readonly Dictionary<string, string> DisplayNames = new Dictionary<string, string>
    {
        { InventoryArtefact, "Wandering Artifact" },
        { Gun, "Revolver" },
        { HarmonicRow, "Harmonic Row" },
        { SonoceramicShard, "Sonoceramic Shard" },
        { SonusGuideTube, "Sonus Guide Tube" },
        { ReceiptWhisperer, "Receipt Whisperer" },
        { WaxStoppers, "Wax Stoppers" },
        { MaintScrollHum, "Maintenance Scroll: Hum" },
        { VentFiddle, "Vent Fiddle" },
        { EarPressureReports, "Ear Pressure Reports" },
        { TestCube, "Test Cube" },
        { Mop, "Broom" },
        { CubesNote, "Crumpled Note" },
        { JapaneseSweets, "Japanese Sweets" },
        { RatkoThings, "Ratko's Things" }
    };

    public static readonly Dictionary<string, string> Descriptions = new Dictionary<string, string>
    {
        { InventoryArtefact, "wandering artifact scribbles" },
        { Mop, "It's a broom! What else to say?" },
        { TestCube, "cool cube" },
        { JapaneseSweets, "Legendary artefact. Or not." },
        { CubesNote, "A strange note on crumpled paper.\n\"They will alter the course of time. Perfect hexahedrons, in a divine quantity. Exquisite sensations for those who are worthy. Parting is painful, yet inevitable.\"\nAlright, that sounds stupid. I'm looking for cubes. With a ridiculous description." },
        { Gun, "It is mine." },
        { HarmonicRow, "gobbledygook melody of squirrels" },
        { SonoceramicShard, "fragment of whispering teapots" },
        { SonusGuideTube, "tube guiding sounds of marshmallows" },
        { ReceiptWhisperer, "coupon mumbo jumbo mosaic" },
        { WaxStoppers, "sticky nonsense of waxy blobs" },
        { MaintScrollHum, "humming scroll of baffling fumes" },
        { VentFiddle, "perplexing fiddle for vents" },
        { EarPressureReports, "reports full of earwig gibberish" },
        { RatkoThings, "I have no idea what this is." }
    };

    public static readonly Dictionary<string, string> ImagePaths = new Dictionary<string, string>
    {
        { InventoryArtefact, "Images/InventoryArtefact" },
        { TestCube, "Images/TestCube" },
        { JapaneseSweets, "Images/JapaneseSweets" },
        { Gun, "Images/Gun" },
        { HarmonicRow, "Images/HarmonicRow" },
        { SonoceramicShard, "Images/SonoceramicShard" },
        { SonusGuideTube, "Images/SonusGuideTube" },
        { ReceiptWhisperer, "Images/ReceiptWhisperer" },
        { WaxStoppers, "Images/WaxStoppers" },
        { MaintScrollHum, "Images/MaintScrollHum" },
        { VentFiddle, "Images/VentFiddle" },
        { EarPressureReports, "Images/EarPressureReports" },
        { Mop, "Images/Broom" },
        { CubesNote, "Images/CubesNote" }
    };
}
