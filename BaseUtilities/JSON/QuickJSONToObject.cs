﻿/*
 * Copyright © 2020 robby & EDDiscovery development team
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed under
 * the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND, either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 * 
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */

using System;

namespace BaseUtils.JSON
{
    public static class JTokenExtensions
    {
        public static T ToObject<T>(this JToken tk)                 // returns null if not decoded
        {
            Type tt = typeof(T);
            Object ret = tk.ToObject(tt);
            if (ret.GetType() != tt)        // null if not returned T
                return default(T);
            else
                return (T)ret;
        }

        public static T ToObjectProtected<T>(this JToken tk)        // backwards compatible.. maybe need to try/catch wrap? Not sure yet
        {
            Type tt = typeof(T);
            Object ret = tk.ToObject(tt);
            if (ret.GetType() != tt)        // null if not returned T
                return default(T);
            else
                return (T)ret;
        }

        public class ToObjectError { public string ErrorString; public ToObjectError(string s) { ErrorString = s; } };

        // returns Object of type tt, or ToObjectError, or null if tk == JNotPresent.

        public static Object ToObject(this JToken tk, Type tt)       // will return an instance of tt or ToObjectError, or null for not JNotPresent
        {
            if (tk.IsString)
            {
                if (tt == typeof(string))
                {
                    return (string)tk.Value;
                }
                else
                    return new ToObjectError("Not string");
            }
            else if (tk.IsInt)
            {
                if (tt == typeof(int))
                {
                    return tk.Int();
                }
                else if (tt == typeof(uint))
                {
                    return tk.UInt();
                }
                else if (tt == typeof(long))
                {
                    return tk.Long();
                }
                else if (tt == typeof(ulong))
                {
                    return tk.ULong();
                }
                else
                    return new ToObjectError("Not int");
            }
            else if (tk.IsBool)
            {
                if (tt == typeof(bool))
                {
                    return (bool)tk.Value;
                }
                else
                    return new ToObjectError("Not bool");
            }
            else if (tk.IsDouble)
            {
                if (tt == typeof(double))
                {
                    return (double)tk.Value;
                }
                else
                    return new ToObjectError("Not double");
            }
            else if (tk.IsNull)
            {
                if (tt == typeof(string))
                {
                    return null;
                }
                else
                    return new ToObjectError("JNull must be assigned to string");
            }
            else if (tk.IsArray)
            {
                if (tt.IsArray)
                {
                    dynamic instance = Activator.CreateInstance(tt, tk.Count);   // dynamic holder for instance of array[]

                    JArray jarray = (JArray)tk;

                    for (int i = 0; i < tk.Count; i++)
                    {
                        Object ret = ToObject(tk[i], tt.GetElementType());      // get the underlying element

                        if (ret.GetType() == typeof(ToObjectError))
                            return ret;
                        else
                        {
                            dynamic d = Convert.ChangeType(ret, tt.GetElementType());       // convert to element type, which should work since we checked compatibility
                            instance[i] = d;
                        }
                    }

                    return instance;
                }
                else
                    return new ToObjectError("Not array");
            }
            else if (tk.TokenType == JToken.TType.Object)                   // objects are best efforts.. fills in as many fields as possible
            {
                if (tt.IsClass)
                {
                    var instance = Activator.CreateInstance(tt);        // create the class, so class must has a constructor with no paras

                    var members = tt.GetMembers(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static |
                                                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

                    foreach (var kvp in (JObject)tk)
                    {
                        var pos = System.Array.FindIndex(members, x => x.Name == kvp.Key);

                        if (pos >= 0)                                   // if we found a class member
                        {
                            var mi = members[pos];
                            Type otype = mi.FieldPropertyType();

                            if (otype != null)                          // and its a field or property
                            {
                                Object ret = ToObject(kvp.Value, otype);    // get the value

                                if (ret.GetType() == typeof(ToObjectError))
                                    return ret;
                                else
                                    mi.SetValue(instance, ret);         // and set. 
                            }
                        }
                    }

                    return instance;
                }
                else
                    return new ToObjectError("Not class");
            }
            else if (tk.IsNotPresent)       // if its purposely not present, return null
                return null;
            else
                return new ToObjectError("Unknown type of JToken");
        }
    }
}



