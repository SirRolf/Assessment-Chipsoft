namespace Assessment_Chipsoft.Records;

/// <summary>
/// Information containing name of document and Directory towards the location of the document
/// </summary>
/// <param name="Name">FileName of the document provided</param>
/// <param name="Directory">FilePath to where the Document is stored</param>
public record DocumentInfo(string Name, string Directory);