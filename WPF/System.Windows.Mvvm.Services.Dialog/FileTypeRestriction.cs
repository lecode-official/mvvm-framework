
#region Using Directives

using System.Collections.Generic;
using System.Linq;

#endregion

namespace System.Windows.Mvvm.Services.Dialog
{
    /// <summary>
    /// A class that determines the restrictions on file types for file open and file save dialogs.
    /// </summary>
    public class FileTypeRestriction
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="FileTypeRestriction"/> instance.
        /// </summary>
        /// <param name="fileTypesDescription">The description for the restriction (e.g. "Text documents").</param>
        /// <param name="fileTypes">The file extensions (without dot and star, e.g. "txt" instead of "*.txt") of the file types on which the file open and file save dialogs should be restricted.</param>
        public FileTypeRestriction(string fileTypesDescription, params string[] fileTypes)
        {
            this.FileTypesDescription = fileTypesDescription;
            this.FileTypes = fileTypes.ToList();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the description for the file type restrictions.
        /// </summary>
        public string FileTypesDescription { get; private set; }

        /// <summary>
        /// Gets the file types, that the file open and file save dialog should be restricted on
        /// </summary>
        public IEnumerable<string> FileTypes { get; private set; }

        #endregion

        #region Public Static Properties

        /// <summary>
        /// Gets the file type restriction for image files (bitmaps, jpegs and pngs).
        /// </summary>
        public static FileTypeRestriction ImageFiles
        {
            get
            {
                return new FileTypeRestriction(Resources.Localization.FileTypeRestriction.ImageFilesRestrictionDescription, "bmp", "jpg", "png");
            }
        }

        /// <summary>
        /// Gets the file type restriction for files files (txt).
        /// </summary>
        public static FileTypeRestriction TextFiles
        {
            get
            {
                return new FileTypeRestriction(Resources.Localization.FileTypeRestriction.TextFilesRestrictionDescription, "txt");
            }
        }

        /// <summary>
        /// Gets the file type restriction for all files (*.*).
        /// </summary>
        public static FileTypeRestriction AllFiles
        {
            get
            {
                return new FileTypeRestriction(Resources.Localization.FileTypeRestriction.AllFilesRestrictionDescription, "*");
            }
        }

        /// <summary>
        /// Gets the file type restriction for video files (MP4).
        /// </summary>
        public static FileTypeRestriction VideoFiles
        {
            get
            {
                return new FileTypeRestriction(Resources.Localization.FileTypeRestriction.VideoFilesRestrictionDescription, "mp4");
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the string representation for the file type restriction.
        /// </summary>
        public string Restriction
        {
            get
            {
                return string.Concat(this.FileTypesDescription, "|", string.Join(";", this.FileTypes.Select(fileType => string.Concat("*.", fileType))));
            }
        }

        #endregion
    }
}