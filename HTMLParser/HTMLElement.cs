using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HTMLParser
{
    // This class holds all the html tags that dont allow tag to have
    // child nodes
    // Right now I am not making use of this. 
    // NOTE: FUTURE USE
    public static class ElementTypesWithChildNotAllowed
    {
        public static ElementType[] Nodes = new ElementType[]{
                                                    ElementType.param, 
                                                    ElementType.basefont, 
                                                    ElementType.@base, 
                                                    ElementType.col, 
                                                    ElementType.link, 
                                                    ElementType.area, 
                                                    ElementType.img, 
                                                    ElementType.frame, 
                                                    ElementType.input, 
                                                    ElementType.hr, 
                                                    ElementType.br,
                                                    ElementType.button,
                                                    ElementType.@text
                                                    };
    }

    // This class holds all the html tags 
    // Right now I am not making use of this. 
    // NOTE: FUTURE USE
    public enum ElementType
    {
        abbr,
        acronym,
        address,
        area,
        b,
        @base,
        basefont,
        bdo,
        big,
        blockquote,
        body,
        br,
        button,
        caption,
        center,
        cite,
        code,
        col,
        colgroup,
        dd,
        del,
        dfn,
        div,
        dl,
        dt,
        em,
        fieldset,
        font,
        form,
        frame,
        frameset,
        h1,
        h2,
        h3,
        h4,
        h5,
        h6,
        head,
        hr,
        html,
        i,
        iframe,
        img,
        input,
        ins,
        kbd,
        label,
        legend,
        li,
        link,
        map,
        menu,
        meta,
        noframes,
        noscript,
        @object,
        ol,
        optgroup,
        option,
        p,
        param,
        pre,
        q,
        samp,
        script,
        select,
        small,
        span,
        strike,
        strong,
        style,
        sub,
        sup,
        table,
        tbody,
        td,
        textarea,
        @text,
        tfoot,
        th,
        thead,
        title,
        tr,
        tt,
        u,
        ul,
        var,
        comment,
        none,
        invalid,
        cdata
    }

    public class KeyValueEx
    {
        public String Key { get; set; }
        public String Value { get; set; }
    }
    public class HTMLElement
    {
        //This enumeration tells what type the node is
        //Right now I am setting this to ElementType.None. FUTURE USE 
        public ElementType TagType { get; set; }
        
        // This is the String representation tag e.g. a, br, table
        public String Tag { get; set; }
        
        // This is the whole tag including attributes and values
        // eg.e <table border=1 > ....
        // NOTE: THERE IS NO CLOSING NODE IN DOM TREE. E.G ONE WONT FIND </table> in DOM TREE
        public String Text { get; set; }
        
        // This tell if the node supports having child nodes.
        public Boolean CanHaveChildren { get; set; }

        //
        public Boolean HasNoStartNode{ get; set; }

        // Parent of current Node
        public HTMLElement Parent { get; set; }

        // Attributes of the Node
        private List<KeyValueEx> attributes = null;

        public List<KeyValueEx> Attributes
        {
            get { return attributes; }
            set { attributes = value; }
        }
        private List<HTMLElement> childNodes = null;

        // CHild Nodes for Current Node
        public List<HTMLElement> ChildNodes
        {
            get { return childNodes; }
            set { childNodes = value; }
        }

        public HTMLElement()
        {
            this.attributes = new List<KeyValueEx>();
            this.childNodes = new List<HTMLElement>();
            this.Tag = String.Empty;
            this.Text = String.Empty;
            this.TagType = ElementType.none;
            this.CanHaveChildren = true;
            this.HasNoStartNode = false;
        }

    }
}
