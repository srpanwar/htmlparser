using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace HTMLParser
{
    /// <summary>
    ///  This is the main Parser class
    /// </summary>
    public class Parser
    {
        // This is retrieved html
        public String Html { get; set; }
        
        // NOTE: FUTURE USE
        public List<String> Scripts { get; set; }
        
        // Document for retrieved Html
        public Document HDocument { get; set; }
        private HTMLElement _currentNode = null;

        /// <summary>
        /// Parses the giveen Html for form the
        /// DOM Tree.
        /// </summary>
        /// <param name="html"></param>
        public void Parse(String html)
        {
            this.Html = html;
            this.HDocument = new Document ();
            this.Scripts = new List<string>();
            this.CreateDom();
        }

        /// <summary>
        /// Create the DOM 
        /// </summary>
        private void CreateDom()
        {
            if (String.IsNullOrEmpty(this.Html))
                return;
            int index = 0;
            // Form the Header Element first
            FormHeader(ref index);
            this._currentNode = this.HDocument.Root;
            // Form the Rest of the DOM
            FormElement(ref index);
            return;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="index"></param>
        private void FormElement(ref int index)
        {
            while (index < this.Html.Length)
            {
                // start keeps track of the starting index value
                // this is important because if at end this loop occurence 
                // if the new index value is same as start value then we found
                // a node that is not valid html node
                int start = index; 

                #region Form the #Text Node
                //Skip till we are start of element node & form the text node
                StringBuilder textElementBuilder = new StringBuilder();
                while (index < this.Html.Length && this.Html[index] != '<') 
                {
                    textElementBuilder.Append(this.Html[index]);
                    index++; 
                }
                if (textElementBuilder.Length > 0)
                {
                    HTMLElement textElement = new HTMLElement(); // The text node for current html node
                    textElement.Tag = "#text";
                    textElement.TagType = ElementType.text;
                    textElement.CanHaveChildren = false;
                    textElement.Parent = _currentNode;
                    textElement.Text = textElementBuilder.ToString();
                    _currentNode.ChildNodes.Add(textElement);
                }
                #endregion

                // Check if we reached the end of Htnl String
                if (index >= this.Html.Length)
                    return;
                
                // NOTE this.Html[index] HERE SHOULD ALWAYS POINT TO START OF THE NODE '<'
                // OR END OF this.Html STRING

                #region Form CDATA or Comment Node
                //Processing Comment Node || CDATA node
                if ((index + 1 < this.Html.Length) && this.Html[index + 1] == '!')
                {
                    //Processing Comment Node 
                    if (((index + 2 < this.Html.Length) && this.Html[index + 2] == '-') &&
                        ((index + 3 < this.Html.Length) && this.Html[index + 3] == '-'))
                    {
                        FormComment(_currentNode, ref index);
                        continue;
                    }

                    //Processing CDATA Node  <![CDATA[
                    if (((index + 2 < this.Html.Length) && this.Html[index + 2] == '[') &&
                        ((index + 3 < this.Html.Length) && this.Html[index + 3] == 'C') &&
                        ((index + 4 < this.Html.Length) && this.Html[index + 4] == 'D') &&
                        ((index + 5 < this.Html.Length) && this.Html[index + 5] == 'A') &&
                        ((index + 6 < this.Html.Length) && this.Html[index + 6] == 'T') &&
                        ((index + 7 < this.Html.Length) && this.Html[index + 7] == 'A') &&
                        ((index + 8 < this.Html.Length) && this.Html[index + 8] == '[')
                        )
                    {
                        FormCDATA(_currentNode, ref index);
                        continue;
                    }
                }
                #endregion

                #region Form Other Html Nodes
                //Skip for all spaces between '<' and 1st tag alphabet e.g. "<  br/>
                int tmpIndex = index + 1;
                StringBuilder tag = new StringBuilder();
                while (tmpIndex < this.Html.Length && this.Html[tmpIndex] == ' ')
                { tmpIndex++; }

                //Check if this node is Closing Node e.g. </table>
                // Or else form the tag e.g <table border=1 > : here tag is 'table'
                Boolean isClosingNode = false;
                while (tmpIndex < this.Html.Length && this.Html[tmpIndex] != ' ' && this.Html[tmpIndex] != '<' && this.Html[tmpIndex] != '>')
                {
                    if (this.Html[tmpIndex] == '/')
                    {
                        if (tag.Length == 0)
                        {
                            isClosingNode = true;
                            tmpIndex++;
                            continue;
                        }
                        else
                            break;
                    }

                    if (this.Html[tmpIndex] != ' ')
                        tag.Append(this.Html[tmpIndex]);
                    tmpIndex++; 
                }

                while (isClosingNode && tmpIndex < this.Html.Length && this.Html[tmpIndex] != '<' && this.Html[tmpIndex] != '>')
                    tmpIndex++;
                if (isClosingNode && tmpIndex < this.Html.Length && this.Html[tmpIndex] == '>')
                    tmpIndex++;

                // we found a tag here? 
                if (tag.Length > 0)
                {
                    String tagfix = tag.ToString().ToLower();
                    if (isClosingNode && _currentNode.CanHaveChildren)
                    {
                        if (_currentNode.Tag == tagfix)
                        {
                            // This is the closing tag for current node, so we are done with this node. time to return
                            // if this is closing tag and belong to some other node (malformed html) we simply ignore it.
                            this._currentNode = _currentNode.Parent;
                        }
                        else
                        {
                            //Add an Orphan Node Or Node with No Start Node
                            HTMLElement orphanElement = new HTMLElement();
                            orphanElement.Tag = tagfix;
                            orphanElement.Text = this.Html.Substring(index, tmpIndex - index);
                            orphanElement.TagType = ElementType.none;
                            orphanElement.CanHaveChildren = false;
                            orphanElement.HasNoStartNode = true;
                            orphanElement.Parent = _currentNode;
                            _currentNode.ChildNodes.Add(orphanElement);
                        }
                        index = tmpIndex;
                    }
                    else
                    {
                        // We handle Nodes with no child allowed in FormNoChildNode method
                        if (tagfix == "param" || tagfix == "basefont" || tagfix == "base" || tagfix == "col" || tagfix == "link" || tagfix == "area" || tagfix == "img" || tagfix == "frame" || tagfix == "input" || tagfix == "hr" || tagfix == "br" || /*tagfix == "button" ||*/ tagfix == "meta")
                        {
                            HTMLElement inputElement = new HTMLElement();
                            inputElement.Tag = tagfix;
                            inputElement.TagType = ElementType.none;
                            inputElement.CanHaveChildren = false;
                            inputElement.Parent = _currentNode;
                            _currentNode.ChildNodes.Add(inputElement);
                            FormNoChildNode(inputElement, ref index);
                        }
                        else
                        {
                            // We handle Script node in FormScriptNode method
                            if (tagfix == "script")
                            {
                                HTMLElement scriptElement = new HTMLElement();
                                scriptElement.Tag = tagfix;
                                scriptElement.TagType = ElementType.none;
                                scriptElement.CanHaveChildren = true;
                                scriptElement.Parent = _currentNode;
                                _currentNode.ChildNodes.Add(scriptElement);
                                FormScriptNode(scriptElement, ref index);
                            }
                            else
                            {
                                //All other nodes are handled here
                                HTMLElement otherElement = new HTMLElement();
                                otherElement.Tag = tagfix;
                                otherElement.TagType = ElementType.none;
                                otherElement.CanHaveChildren = true;
                                otherElement.Parent = _currentNode;
                                _currentNode.ChildNodes.Add(otherElement);
                                FormWithChildNode(otherElement, ref index);
                            }
                        }
                    }
                }
                #endregion

                if (start == index)
                    index++;
            }
            return;
        }

        /// <summary>
        /// This method is used to parse the node which allow for child nodes.
        /// NOTE: There is not significant difference in parsing here and method for nodes that cannot have child nodes.
        ///       This is for simplicity of understanding.
        /// </summary>
        /// <param name="inputElement"></param>
        /// <param name="parent"></param>
        /// <param name="index"></param>
        private void FormWithChildNode(HTMLElement otherElement, ref int index)
        {
            StringBuilder elemBuilder = new StringBuilder();
            elemBuilder.Append("<" + otherElement.Tag);
            index = index + otherElement.Tag.Length + 1;
            while (index < this.Html.Length)
            {
                if (index < this.Html.Length && this.Html[index] == ' ')
                { elemBuilder.Append(this.Html[index]); index++; continue; }

                if (index >= this.Html.Length)
                    break;

                //forming the attribute nodeName 
                StringBuilder attrBuilder = new StringBuilder();
                while (index < this.Html.Length && (this.Html[index] != '=' && this.Html[index] != '>' && this.Html[index] != '<'))
                {
                    if (this.Html[index] == '/' && (index + 1 < this.Html.Length) && this.Html[index + 1] == '>')
                    { index++; continue; }

                    elemBuilder.Append(this.Html[index]);
                    attrBuilder.Append(this.Html[index]);
                    index++;
                }

                if (index >= this.Html.Length)
                    break;

                //handle different element ending case here
                if (this.Html[index] == '>' || this.Html[index] == '<')
                {
                    if (this.Html[index] == '>')
                    {
                        index++;
                        if (this.Html[index - 2] != '/')
                        {
                            elemBuilder.Append("/>");
                            otherElement.Text = elemBuilder.ToString();
                            this._currentNode = otherElement;
                        }
                    }
                    return;
                }

                elemBuilder.Append(this.Html[index]);// this is for '=' case
                index++; // this is for '=' case
                //forming the value nodeValue
                StringBuilder valueBuilder = new StringBuilder();
                bool ignoreSpace = false;
                bool startSpaceSkip = true;
                char startChar = default(char);
                while (index < this.Html.Length && ((this.Html[index] != '>' && this.Html[index] != '<') || ignoreSpace))
                {
                    if (this.Html[index] == ' ' && startSpaceSkip)
                    { index++; continue; }
                    startSpaceSkip = false;

                    if (valueBuilder.Length == 0 && (this.Html[index] == '"' || this.Html[index] == '\'') && ignoreSpace == false)
                    { ignoreSpace = true; elemBuilder.Append(this.Html[index]); startChar = this.Html[index]; index++; continue; }

                    if (this.Html[index] == startChar && ignoreSpace == true)
                    { ignoreSpace = false; elemBuilder.Append(this.Html[index]); index++; break; }

                    if (this.Html[index] == ' ' && ignoreSpace == false)
                    { break; }

                    if (this.Html[index] == '/' && (index + 1 < this.Html.Length) && this.Html[index + 1] == '>')
                    { index++; continue; }

                    elemBuilder.Append(this.Html[index]);
                    valueBuilder.Append(this.Html[index]);
                    index++;
                }

                //add nodeValue to element
                if (valueBuilder.Length > 0)
                {
                    otherElement.Attributes.Add(new KeyValueEx() { Key = attrBuilder.ToString().Trim(), Value = valueBuilder.ToString() });
                }

                if (index >= this.Html.Length)
                    break;

                //handle different element ending case here
                if (this.Html[index] == '>' || this.Html[index] == '<')
                {
                    if (this.Html[index] == '>')
                    {
                        index++;
                        if (this.Html[index - 2] != '/')
                        {
                            elemBuilder.Append("/>");
                            otherElement.Text = elemBuilder.ToString();
                            this._currentNode = otherElement;
                        }
                    }
                    return;
                }
            }

            if (index >= this.Html.Length)
            {
                otherElement.Text = elemBuilder.ToString();
                return;
            }

            if (this.Html[index] == '>' || this.Html[index] == '<')
            {
                if (this.Html[index - 1] == '/')
                    elemBuilder.Append("/");
                if (elemBuilder[elemBuilder.Length - 1] != '>')
                    elemBuilder.Append(">");
                otherElement.Text = elemBuilder.ToString();
                if (this.Html[index] == '>')
                    index++;
                return;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputElement"></param>
        /// <param name="parent"></param>
        /// <param name="index"></param>
        private void FormScriptNode(HTMLElement inputElement, ref int index)
        {
            bool scriptEnded = false;
            StringBuilder elemBuilder = new StringBuilder();
            elemBuilder.Append("<" + inputElement.Tag);
            index = index + inputElement.Tag.Length + 1;
            while (index < this.Html.Length)
            {
                if (index < this.Html.Length && this.Html[index] == ' ')
                { elemBuilder.Append(this.Html[index]); index++; continue; }

                if (index >= this.Html.Length)
                    break;
                //elemBuilder.Append(" ");

                //forming the attribute nodeName 
                StringBuilder attrBuilder = new StringBuilder();
                while (index < this.Html.Length && (this.Html[index] != '=' && this.Html[index] != '>' && this.Html[index] != '<'))
                {
                    if (this.Html[index] == '/' && (index + 1 < this.Html.Length) && this.Html[index + 1] == '>')
                    { scriptEnded = true; index++; continue; }

                    elemBuilder.Append(this.Html[index]);
                    attrBuilder.Append(this.Html[index]);
                    index++;
                }

                if (index >= this.Html.Length)
                    break;

                if (this.Html[index] == '>' || this.Html[index] == '<')
                {
                    break;
                }

                elemBuilder.Append(this.Html[index]);// this is for '=' case
                index++; // this is for '=' case
                //forming the value nodeValue
                StringBuilder valueBuilder = new StringBuilder();
                bool ignoreSpace = false;
                bool startSpaceSkip = true;
                char startChar = default(char);
                while (index < this.Html.Length && ((this.Html[index] != '>' && this.Html[index] != '<') || ignoreSpace))
                {
                    if (this.Html[index] == ' ' && startSpaceSkip)
                    { index++; continue; }
                    startSpaceSkip = false;

                    if (valueBuilder.Length == 0 && (this.Html[index] == '"' || this.Html[index] == '\'') && ignoreSpace == false)
                    { elemBuilder.Append(this.Html[index]); ignoreSpace = true; startChar = this.Html[index]; index++; continue; }

                    if (this.Html[index] == startChar && ignoreSpace == true)
                    { elemBuilder.Append(this.Html[index]); index++; break; }

                    if (this.Html[index] == ' ' && ignoreSpace == false)
                    { break; }

                    if (this.Html[index] == '/' && (index + 1 < this.Html.Length) && this.Html[index + 1] == '>')
                    { scriptEnded = true; index++; continue; }

                    elemBuilder.Append(this.Html[index]);
                    valueBuilder.Append(this.Html[index]);
                    index++;
                }
                //elemBuilder.Append(" ");

                if (valueBuilder.Length > 0)
                {
                    inputElement.Attributes.Add(new KeyValueEx() { Key = attrBuilder.ToString().Trim(), Value = valueBuilder.ToString() });
                }

                if (index >= this.Html.Length)
                    break;

                if (this.Html[index] == '>' || this.Html[index] == '<')
                {
                    break;
                }
            }

            if (index >= this.Html.Length)
            {
                inputElement.Text = elemBuilder.ToString();
                return;
            }

            if (this.Html[index] == '>' || this.Html[index] == '<')
            {
                elemBuilder.Append("/>");
                inputElement.Text = elemBuilder.ToString();
                if (this.Html[index] == '>')
                    index++;
            }
            if (scriptEnded == false)
            {
                HTMLElement textElement = new HTMLElement(); // The text node for current html node
                textElement.Tag = "#text";
                textElement.TagType = ElementType.text;
                textElement.CanHaveChildren = false;
                textElement.Parent = _currentNode;
                inputElement.ChildNodes.Add(textElement);
                StringBuilder scriptText = new StringBuilder();
                
                bool insideString = false;
                char startChar = default(char);
                while (index < this.Html.Length)
                {
                    if (!insideString && (this.Html[index] == '\'' || this.Html[index] == '"'))
                    { insideString = true; startChar = this.Html[index]; }

                    if (insideString && this.Html[index] == startChar)
                    { insideString = false; startChar = default(char); }
                    
                    if (!insideString)
                    {
                        if (index < this.Html.Length && this.Html[index] == '<' &&
                        index + 1 < this.Html.Length && (this.Html[index + 1] == '/') &&
                        index + 2 < this.Html.Length && (this.Html[index + 2] == 's' || this.Html[index + 2] == 'S') &&
                        index + 3 < this.Html.Length && (this.Html[index + 3] == 'c' || this.Html[index + 3] == 'C') &&
                        index + 4 < this.Html.Length && (this.Html[index + 4] == 'r' || this.Html[index + 4] == 'R') &&
                        index + 5 < this.Html.Length && (this.Html[index + 5] == 'i' || this.Html[index + 5] == 'I') &&
                        index + 6 < this.Html.Length && (this.Html[index + 6] == 'p' || this.Html[index + 6] == 'T') &&
                        index + 7 < this.Html.Length && (this.Html[index + 7] == 't' || this.Html[index + 7] == '>') &&
                        index + 8 < this.Html.Length && (this.Html[index + 8] == '>')
                        )
                        {
                            scriptEnded = true;
                            index = index + 9;
                            break;
                        }
                    }
                    //if (index < this.Html.Length && this.Html[index] == '<')
                      //  break;
                    scriptText.Append(this.Html[index]);
                    index++;
                }
                textElement.Text = scriptText.ToString();
            }
            return;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputElement"></param>
        /// <param name="parent"></param>
        /// <param name="index"></param>
        private void FormNoChildNode(HTMLElement inputElement, ref int index)
        {
            StringBuilder elemBuilder = new StringBuilder();
            elemBuilder.Append("<" + inputElement.Tag);
            index = index + inputElement.Tag.Length + 1;
            while (index < this.Html.Length)
            {
                if (index < this.Html.Length && this.Html[index] == ' ')
                { elemBuilder.Append(this.Html[index]); index++; continue; }

                if (index >= this.Html.Length)
                    break;

                //forming the attribute nodeName 
                StringBuilder attrBuilder = new StringBuilder();
                while (index < this.Html.Length && (this.Html[index] != '=' && this.Html[index] != '>' && this.Html[index] != '<'))
                {
                    if (this.Html[index] == '/' && (index + 1 < this.Html.Length) && this.Html[index + 1] == '>')
                    { index++; continue; }

                    elemBuilder.Append(this.Html[index]);
                    attrBuilder.Append(this.Html[index]);
                    index++;
                }

                if (index >= this.Html.Length)
                    break;


                if (this.Html[index] == '>' || this.Html[index] == '<')
                {
                    break;
                }

                elemBuilder.Append(this.Html[index]);// this is for '=' case
                index++; // this is for '=' case
                //forming the value nodeValue
                StringBuilder valueBuilder = new StringBuilder();
                bool ignoreSpace = false;
                bool startSpaceSkip = true;
                char startChar = default(char);
                while (index < this.Html.Length && ((this.Html[index] != '>' && this.Html[index] != '<') || ignoreSpace))
                {
                    if (this.Html[index] == ' ' && startSpaceSkip)
                    { index++; continue; }
                    startSpaceSkip = false;

                    if (valueBuilder.Length == 0 && (this.Html[index] == '"' || this.Html[index] == '\'') && ignoreSpace == false)
                    { elemBuilder.Append(this.Html[index]); ignoreSpace = true; startChar = this.Html[index]; index++; continue; }

                    if (this.Html[index] == startChar && ignoreSpace == true)
                    { elemBuilder.Append(this.Html[index]); index++; break; }

                    if (this.Html[index] == ' ' && ignoreSpace == false)
                    { break; }

                    if (this.Html[index] == '/' && (index + 1 < this.Html.Length) && this.Html[index + 1] == '>')
                    { index++; continue; }

                    elemBuilder.Append(this.Html[index]);
                    valueBuilder.Append(this.Html[index]);
                    index++;
                }

                if (valueBuilder.Length > 0)
                {
                    //inputElement.Attributes[attrBuilder.ToString().Trim()] = valueBuilder.ToString();
                    inputElement.Attributes.Add(new KeyValueEx() { Key = attrBuilder.ToString().Trim(), Value = valueBuilder.ToString() });
                }

                if (index >= this.Html.Length)
                    break;

                if (this.Html[index] == '>' || this.Html[index] == '<')
                {
                    break;
                }
            }

            if (index >= this.Html.Length)
            {
                inputElement.Text = elemBuilder.ToString();    
                return;
            }

            if (this.Html[index] == '>' || this.Html[index] == '<')
            {
                elemBuilder.Append("/>");
                inputElement.Text = elemBuilder.ToString();
                if (this.Html[index] == '>')
                    index++;
                return;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="index"></param>
        private void FormComment(HTMLElement parent, ref int index)
        {
            StringBuilder elemBuilder = new StringBuilder();
            elemBuilder.Append("<!--");
            index = index + 4;
            while (index < this.Html.Length)
            {
                if ((index + 2 < this.Html.Length) &&
                    (this.Html[index] == '-' &&
                    (this.Html[index + 1] == '-') &&
                    (this.Html[index + 2] == '>')))
                {
                    elemBuilder.Append("-->");
                    HTMLElement comment = new HTMLElement();
                    comment.Tag = "comment";
                    comment.Text = elemBuilder.ToString();
                    comment.TagType = ElementType.comment;
                    comment.CanHaveChildren = false;
                    parent.ChildNodes.Add(comment);
                    comment.Parent = parent;
                    index = index + 3;
                    return;
                }
                elemBuilder.Append(this.Html[index]);
                index++;
            }

            if (index >= this.Html.Length)
                return;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="index"></param>
        private void FormCDATA(HTMLElement parent, ref int index)
        {
            StringBuilder elemBuilder = new StringBuilder();
            elemBuilder.Append("<![CDATA[");
            index = index + 9;
            while (index < this.Html.Length)
            {
                if ((index + 2 < this.Html.Length) &&
                    (this.Html[index] == ']' &&
                    (this.Html[index + 1] == ']') &&
                    (this.Html[index + 2] == '>')))
                {
                    elemBuilder.Append("]]>");
                    HTMLElement comment = new HTMLElement();
                    comment.Tag = "comment";
                    comment.Text = elemBuilder.ToString();
                    comment.TagType = ElementType.cdata;
                    comment.CanHaveChildren = false;
                    parent.ChildNodes.Add(comment);
                    comment.Parent = parent;
                    index = index + 3;
                    return;
                }
                elemBuilder.Append(this.Html[index]);
                index++;
            }

            if (index >= this.Html.Length)
                return;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        private void FormHeader(ref int index)
        {
            StringBuilder header = new StringBuilder();
            while (index < this.Html.Length && this.Html[index] != '<') { index++; }
            if (index >= this.Html.Length)
            {
                this.HDocument.Header = String.Empty;
                return;
            }

            if ((index + 1 < this.Html.Length) && this.Html[index + 1] == '!')
            {
                header.Append(this.Html[index]);
                index++;
                while (index < this.Html.Length && this.Html[index] != '>' && this.Html[index] != '<')
                {
                    header.Append(this.Html[index]);
                    index++;
                }

                if (index >= this.Html.Length)
                {
                    this.HDocument.Header = header.ToString();
                    return;
                }

                if (this.Html[index] == '<')
                {
                    header.Append('>');
                    this.HDocument.Header = header.ToString();
                    return; // returning from here since we are at opening tag and dont want to ++ index
                }
                header.Append(this.Html[index]);
                this.HDocument.Header = header.ToString();
                index++;
            }

            return;
        }
    }
}
