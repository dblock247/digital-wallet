using System;
using Passbook.Generator.Enums;

namespace Passbook.Generator.Extensions;

public static class PassbookImageExtensions
{
    public static string ToFilename(this PassbookImage passbookImage)
    {
        return passbookImage switch
        {
            PassbookImage.Icon => "icon.png",
            PassbookImage.Icon2X => "icon@2x.png",
            PassbookImage.Icon3X => "icon@3x.png",
            PassbookImage.Logo => "logo.png",
            PassbookImage.Logo2X => "logo@2x.png",
            PassbookImage.Logo3X => "logo@3x.png",
            PassbookImage.Background => "background.png",
            PassbookImage.Background2X => "background@2x.png",
            PassbookImage.Background3X => "background@3x.png",
            PassbookImage.Strip => "strip.png",
            PassbookImage.Strip2X => "strip@2x.png",
            PassbookImage.Strip3X => "strip@3x.png",
            PassbookImage.Thumbnail => "thumbnail.png",
            PassbookImage.Thumbnail2X => "thumbnail@2x.png",
            PassbookImage.Thumbnail3X => "thumbnail@3x.png",
            PassbookImage.Footer => "footer.png",
            PassbookImage.Footer2X => "footer@2x.png",
            PassbookImage.Footer3X => "footer@3x.png",
            _ => throw new NotImplementedException("Unknown PassbookImage type.")
        };
    }
}
