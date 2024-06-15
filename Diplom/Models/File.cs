using System;
using System.Collections.Generic;


public partial class File
{
    public int FileId { get; set; }

    public string FileName { get; set; }

    public string FilePath { get; set; }

    public int Iduser { get; set; }

    public byte[] FileData { get; set; }

    public DateTime CreationDate { get; set; }

}
