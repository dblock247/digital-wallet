namespace Passbook.Generator.Enums;

public enum PassbookImage
{
    /// <summary>
    /// Background image, 180x220 points
    /// </summary>
    Background,

    /// <summary>
    /// @2x Retina background image, 180x220 points
    /// </summary>
    Background2X,

    /// <summary>
    /// @3x Retina background image, 180x220 points
    /// </summary>
    Background3X,

    /// <summary>
    /// Icon as per https://developer.apple.com/library/ios/documentation/UserExperience/Conceptual/MobileHIG/Alerts.html#//apple_ref/doc/uid/TP40006556-CH14
    /// </summary>
    Icon,

    /// <summary>
    /// @2x Retina icon as per https://developer.apple.com/library/ios/documentation/UserExperience/Conceptual/MobileHIG/Alerts.html#//apple_ref/doc/uid/TP40006556-CH14
    /// </summary>
    Icon2X,

    /// <summary>
    /// Retina icon as per https://developer.apple.com/library/ios/documentation/UserExperience/Conceptual/MobileHIG/Alerts.html#//apple_ref/doc/uid/TP40006556-CH14
    /// </summary>
    Icon3X,

    /// <summary>
    /// Logo, 160x50 points
    /// </summary>
    Logo,

    /// <summary>
    /// @2x Retina logo, 160x50 points
    /// </summary>
    Logo2X,

    /// <summary>
    /// @3x Retina logo, 160x50 points
    /// </summary>
    Logo3X,

    /// <summary>
    /// Strip
    /// <list type="bullet">
    ///		<item>
    ///			<description>On iPhone 6 and 6 Plus The allotted space is 375x98 points for event tickets, 375x144 points for gift cards and coupons, and 375x123 in all other cases.</description>
    ///		</item>
    ///		<item>
    ///			<description>On prior hardware The allotted space is 320x84 points for event tickets, 320x110 points for other pass styles with a square barcode on devices with 3.5 inch screens, and 320x123 in all other cases.</description>
    ///		</item>
    ///		<item>
    ///			<description>On iOS 6 The strip image is only 312 points wide; the height varies as described above. A shine effect is automatically applied to the strip image; to suppress it use the suppressStripShine key.</description>
    ///		</item>
    /// </list>
    /// </summary>
    Strip,

    /// <summary>
    /// @2x Retina strip
    /// <list type="bullet">
    ///		<item>
    ///			<description>On iPhone 6 and 6 Plus The allotted space is 375x98 points for event tickets, 375x144 points for gift cards and coupons, and 375x123 in all other cases.</description>
    ///		</item>
    ///		<item>
    ///			<description>On prior hardware The allotted space is 320x84 points for event tickets, 320x110 points for other pass styles with a square barcode on devices with 3.5 inch screens, and 320x123 in all other cases.</description>
    ///		</item>
    ///		<item>
    ///			<description>On iOS 6 The strip image is only 312 points wide; the height varies as described above. A shine effect is automatically applied to the strip image; to suppress it use the suppressStripShine key.</description>
    ///		</item>
    /// </list>
    /// </summary>
    Strip2X,

    /// <summary>
    /// @3x Retina strip
    /// <list type="bullet">
    ///     <item>
    ///         <description>On iPhone 6 and 6 Plus The allotted space is 375x98 points for event tickets, 375x144 points for gift cards and coupons, and 375x123 in all other cases.</description>
    ///     </item>
    ///     <item>
    ///         <description>On prior hardware The allotted space is 320x84 points for event tickets, 320x110 points for other pass styles with a square barcode on devices with 3.5 inch screens, and 320x123 in all other cases.</description>
    ///     </item>
    ///     <item>
    ///         <description>On iOS 6 The strip image is only 312 points wide; the height varies as described above. A shine effect is automatically applied to the strip image; to suppress it use the suppressStripShine key.</description>
    ///     </item>
    /// </list>
    /// </summary>
    Strip3X,

    /// <summary>
    /// Thumbnail, 90x90 points
    /// </summary>
    Thumbnail,

    /// <summary>
    /// @2x Retina thumbnail, 90x90 points
    /// </summary>
    Thumbnail2X,

    /// <summary>
    /// @3x Retina thumbnail, 90x90 points
    /// </summary>
    Thumbnail3X,

    /// <summary>
    /// Footer, 286x15 points
    /// </summary>
    Footer,

    /// <summary>
    /// @2x Retina footer, 286x15 points
    /// </summary>
    Footer2X,

    /// <summary>
    /// @3x Retina footer, 286x15 points
    /// </summary>
    Footer3X
}
