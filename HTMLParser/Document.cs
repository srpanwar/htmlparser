using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HTMLParser
{
    /// <summary>
    /// This Class represents the Html Document.
    /// NOTE: I AM NOT STRICTLY FOLLOWING THE W3C SPEC HERE.
    /// </summary>
    public class Document
    {
        // This is the root node
        public HTMLElement Root { get; set; }
        // Header information for the document
        // e.g. <!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
        public String Header { get; set; }

        public Document()
        {
            this.Root = new HTMLElement();
            this.Header = String.Empty;
        }
    }
}
