﻿using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace VGPrompter {

    public partial class Script {

        internal class TextManager {

            const string
                GLOBAL_TAG = "$",
                GLOBAL_FORMAT = GLOBAL_TAG + ";{0};{1}\n",
                DIALOGUE_FORMAT = "{0};{1};{2}\n";

            static readonly char[] SEPARATOR = new char[1] { ';' };

            MD5 _md5;

            Dictionary<string, Dictionary<string, string>> _dialogue_strings;
            Dictionary<string, string> _global_strings;

            internal Dictionary<string, string> Globals { get { return _global_strings; } }
            internal Dictionary<string, Dictionary<string, string>> Lines { get { return _dialogue_strings; } }

            public string GetHash(string text) {

                var data = _md5.ComputeHash(Encoding.UTF8.GetBytes(text));
                var sb = new StringBuilder();

                foreach (var b in data) {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();

            }

            public TextManager() {
                _md5 = MD5.Create();
                _dialogue_strings = new Dictionary<string, Dictionary<string, string>>();
                _global_strings = new Dictionary<string, string>();
            }

            public string GetText(string label, string hash) {
                return _dialogue_strings[label][hash];
            }

            public string AddText(string label, string text) {
                var hash = GetHash(text);
                if (!_dialogue_strings.ContainsKey(label)) {
                    _dialogue_strings[label] = new Dictionary<string, string>();
                }

                _dialogue_strings[label][hash] = text;

                return hash;
            }

            public void ToCSV(string path) {

                var sb = new StringBuilder();

                foreach (var s in _global_strings) {
                    sb.Append(string.Format(GLOBAL_FORMAT, s.Key, s.Value));
                }

                foreach (var label in _dialogue_strings) {
                    foreach (var s in label.Value) {
                        sb.Append(string.Format(DIALOGUE_FORMAT, label.Key, s.Key, s.Value));
                    }
                }

                File.WriteAllText(path, sb.ToString());
            }

            public void FromCSV(string path) {

                _dialogue_strings.Clear();
                _global_strings.Clear();

                var tokens = new string[3];
                foreach (var row in File.ReadAllLines(path)) {

                    tokens = row.Split(SEPARATOR, 3);
                    var label = tokens[0];
                    var hash = tokens[1];
                    var text = tokens[2];

                    if (label == GLOBAL_TAG) {
                        _global_strings.Add(hash, text);
                    } else {
                        if (!_dialogue_strings.ContainsKey(label)) {
                            _dialogue_strings[label] = new Dictionary<string, string>();
                        }
                        _dialogue_strings[label][hash] = text;
                    }

                }

            }

        }

    }
}
