///
/// Mitchell's Serializable Format
/// Author: Mitchell Garrett
/// Copyright: 2021
///

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FTG.Studios {

    public static class MSF {

        const char semicolon = ';';
        const char assignment = '=';
        const char separator = ',';
        const char open_brace = '{';
        const char close_brace = '}';
        const char open_bracket = '[';
        const char close_bracket = ']';
        const char double_quote = '\"';

        const string singletons = @"(;|=|,|{|}|\[|\]|"")";
        const string identifier = @"^([_a-zA-Z][_a-zA-Z0-9]*)([\._a-zA-Z][_a-zA-Z0-9]*)*$";
        const string number_literal = @"^((\d+(\.\d*)?)|(\.\d+))$";

        // Convert an MSFObject back to a string
        public static string Serialize(MSFObject msf) {
            string value = string.Empty;
            value += open_brace;
            value += '\n';

            // Print all nodes in the object
            foreach (MSFNode node in msf.Nodes) value += Serialize(node);
            value += close_brace;

            return value;
        }

        // Convert an MSFNode back to a string
        public static string Serialize(MSFNode node) {
            string value = string.Empty;

            // Print node key
            value += $"{node.Key} {assignment} ";

            if (node.Value is List<MSFObject>) { // List, print all of its values
                value += open_bracket;
                foreach (MSFObject msf in node.Value as List<MSFObject>) value += Serialize(msf) + separator + " ";
                value += close_bracket;
            } else if (node.Value is string) { // String
                value += double_quote + node.Value.ToString() + double_quote;
            } else { // Anything else
                value += node.Value.ToString();
            }

            value += semicolon;
            value += '\n';
            return value;
        }

        // Convert a string to an MSFObject
        public static MSFObject Serialize(string source) {
            // Get rid of whitespace
            source = Regex.Replace(source, @"(""[^""\\]*(?:\\.[^""\\]*)*"")|\s+", "$1");

            // Tokenize source string
            Queue<string> tokens = new Queue<string>();
            string word = string.Empty;
            foreach (char c in source) {
                if (Regex.IsMatch(c.ToString(), singletons)) {
                    if (!string.IsNullOrEmpty(word)) {
                        tokens.Enqueue(word);
                        word = string.Empty;
                    }

                    tokens.Enqueue(c.ToString());
                    continue;
                }

                word += c;
            }
            return ParseObject(tokens);
        }

        // Generate an MSFObject from a token stream
        static MSFObject ParseObject(Queue<string> tokens) {
            if (!Match(tokens.Peek(), open_brace)) return null;
            tokens.Dequeue();

            MSFObject msf = new MSFObject();
            MSFNode node;

            while ((node = ParseNode(tokens)) != null) msf[node.Key] = node;

            MatchFail(tokens.Dequeue(), close_brace);
            return msf;
        }

        // Generate an MSFNode from a token stream
        static MSFNode ParseNode(Queue<string> tokens) {
            // A node must begin with and identifier and an assignment
            if (!Regex.IsMatch(tokens.Peek(), identifier)) return null;

            MSFNode node = new MSFNode();
            node.Key = tokens.Dequeue();

            MatchFail(tokens.Dequeue(), assignment);

            if (Match(tokens.Peek(), double_quote)) { // String
                tokens.Dequeue();

                node.Value = tokens.Dequeue();

                MatchFail(tokens.Dequeue(), double_quote);
            } else if (Regex.IsMatch(tokens.Peek(), number_literal)) { // Integer
                node.Value = int.Parse(tokens.Dequeue());

            } else if (Match(tokens.Peek(), open_bracket)) { // List
                tokens.Dequeue();

                List<MSFObject> objects = new List<MSFObject>();
                do {
                    MSFObject msf = ParseObject(tokens);
                    if (msf != null) objects.Add(msf);
                } while (Match(tokens.Peek(), separator) && tokens.Dequeue() != null);

                node.Value = objects;

                MatchFail(tokens.Dequeue(), close_bracket);
            } else {
                Fail();
            }

            // A node must end with a semicolon
            MatchFail(tokens.Dequeue(), semicolon);

            return node;
        }

        static bool Match(string token, string expected) {
            return token == expected;
        }

        static bool Match(string token, char expected) {
            return Match(token, expected.ToString());
        }

        static void MatchFail(string token, char expected) {
            if (!Match(token, expected)) Fail();
        }

        static void MatchFail(string token, string expected) {
            if (!Match(token, expected)) Fail();
        }

        static void Fail() {
            Console.WriteLine("Failed to parse MSF.");
        }
    }

    public class MSFObject {
        Dictionary<string, MSFNode> nodes;

        public MSFObject() {
            nodes = new Dictionary<string, MSFNode>();
        }

        public List<string> Keys {
            get { return nodes.Keys.ToList(); }
        }

        public List<MSFNode> Nodes {
            get { return nodes.Values.ToList(); }
        }

        public MSFNode this[string key] {
            get {
                if (nodes.TryGetValue(key, out MSFNode node)) return node;
                return null;
            }

            set { nodes[key] = value; }
        }

        public override string ToString() {
            return MSF.Serialize(this);
        }
    }

    public class MSFNode {
        public string Key;
        public object Value; // Integer, string, list

        public override string ToString() {
            return MSF.Serialize(this);
        }
    }
}