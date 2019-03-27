﻿/*
 * Copyright © 2015 - 2019 EDDiscovery development team
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

using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;

namespace EliteDangerousCore
{
    // For when you need a minimal version and don't want to mess up the database. 
    // Useful for creation of test doubles
    public class SystemClassBase : ISystemBase
    {
        public const float XYZScalar = 128.0F;     // scaling between DB stored values and floats

        static public float IntToFloat(int pos) { return (float)pos / XYZScalar; }
        static public int DoubleToInt(double pos) { return (int)(pos * XYZScalar); }
        static public double IntToDoubleSq(int pos) { double p = (float)pos / XYZScalar; return p * p; }

        public long EDSMID { get; set; }

        public string Name { get; set; }

        public int Xi { get; set; }
        public int Yi { get; set; }
        public int Zi { get; set; }

        public double X { get { return Xi >= int.MinValue ? (double)Xi / XYZScalar : double.NaN; } set { Xi = double.IsNaN(value) ? int.MinValue : (int)(value / XYZScalar); } }
        public double Y { get { return Xi >= int.MinValue ? (double)Yi / XYZScalar : double.NaN; } set { Yi = (int)(value / XYZScalar); } }
        public double Z { get { return Xi >= int.MinValue ? (double)Zi / XYZScalar : double.NaN; } set { Zi = (int)(value / XYZScalar); } }

        public bool HasCoordinate { get { return Xi != int.MinValue; } }

        public int GridID { get; set; }
        public long? SystemAddress { get; set; }

        public bool Equals(ISystemBase other)
        {
            return other != null &&
                   other.Name.Equals(this.Name, StringComparison.InvariantCultureIgnoreCase) &&
                   (!this.HasCoordinate || !other.HasCoordinate ||
                    (Math.Abs(this.X - other.X) < 0.125 &&
                     Math.Abs(this.Y - other.Y) < 0.125 &&
                     Math.Abs(this.Z - other.Z) < 0.125));
        }

        public double Distance(ISystemBase s2)
        {
            if (s2 != null && HasCoordinate && s2.HasCoordinate)
                return Math.Sqrt((X - s2.X) * (X - s2.X) + (Y - s2.Y) * (Y - s2.Y) + (Z - s2.Z) * (Z - s2.Z));
            else
                return -1;
        }

        public bool Distance(ISystemBase s2, double min, double max)
        {
            if (s2 != null && HasCoordinate && s2.HasCoordinate)
            {
                double distsq = (X - s2.X) * (X - s2.X) + (Y - s2.Y) * (Y - s2.Y) + (Z - s2.Z) * (Z - s2.Z);
                return distsq >= min * min && distsq <= max * max;
            }
            else
                return false;
        }

        public double Distance(double ox, double oy, double oz)
        {
            if (HasCoordinate)
                return Math.Sqrt((X - ox) * (X - ox) + (Y - oy) * (Y - oy) + (Z - oz) * (Z - oz));
            else
                return -1;
        }

        public double DistanceSq(double x, double y, double z)
        {
            if (HasCoordinate)
                return (X - x) * (X - x) + (Y - y) * (Y - y) + (Z - z) * (Z - z);
            else
                return -1;
        }

        public bool Cuboid(ISystemBase s2, double min, double max)
        {
            if (s2 != null && HasCoordinate && s2.HasCoordinate)
            {
                double xd = Math.Abs(X - s2.X);
                double yd = Math.Abs(Y - s2.Y);
                double zd = Math.Abs(Z - s2.Z);
                return xd >= min && xd <= max && yd >= min && yd <= max && zd >= min && zd <= max;
            }
            else
                return false;
        }
    }

    [DebuggerDisplay("System {Name} ({X,nq},{Y,nq},{Z,nq})")]
    public class SystemClass : SystemClassBase, ISystem
    {
        public SystemClass()
        {
        }

        public SystemClass(string name)
        {
            Name = name;
            status = SystemStatusEnum.Unknown;
        }

        public SystemClass(long id)
        {
            Name = "UnKnown";
            EDSMID = id;
            status = SystemStatusEnum.Unknown;
        }

        public SystemClass(string Name, double vx, double vy, double vz)
        {
            base.Name = Name;
            status = SystemStatusEnum.Unknown;
            X = vx; Y = vy; Z = vz;
        }

        public SystemClass(string Name, int xi, int yi, int zi, long edsmid, int gridid = -1, string eddbproperties = null)
        {
            base.Name = Name;
            Xi = xi; Yi = yi; Zi = zi;
            EDSMID = edsmid;
            GridID = gridid == -1 ? EliteDangerousCore.DB.GridId.Id(xi, zi) : gridid;
            status = SystemStatusEnum.Unknown;

            if ( eddbproperties != null )
            {
                try
                {
                    JObject jo = JObject.Parse(eddbproperties);
                    EDDBID = jo["id"].Long();
                    Faction = jo["controlling_minor_faction"].Str();
                    Population = jo["population"].Long();
                    Government = EliteDangerousTypesFromJSON.Government2ID(jo["government"]);
                    Allegiance = EliteDangerousTypesFromJSON.Allegiance2ID(jo["allegiance"]);
                    State = EliteDangerousTypesFromJSON.EDState2ID(jo["state"]);
                    Security = EliteDangerousTypesFromJSON.EDSecurity2ID(jo["security"]);
                    PrimaryEconomy = EliteDangerousTypesFromJSON.EDEconomy2ID(jo["primary_economy"]);
                    NeedsPermit = jo["needs_permit"].Int();
                    EDDBUpdatedAt = jo["updated_at"].Int();
                }
                catch ( Exception ex )
                {
                    System.Diagnostics.Debug.WriteLine("EDDB Info stored in db failed " + eddbproperties);
                }
            }
        }

        public SystemStatusEnum status { get; set; }

        public long EDDBID { get; set; }
        public string Faction { get; set; }
        public long Population { get; set; }
        public EDGovernment Government { get; set; }
        public EDAllegiance Allegiance { get; set; }
        public EDState State { get; set; }
        public EDSecurity Security { get; set; }
        public EDEconomy PrimaryEconomy { get; set; }
        public int NeedsPermit { get; set; }
        public int EDDBUpdatedAt { get; set; }

        public bool HasEDDBInformation
        {
            get
            {
                return Population != 0 || Government != EDGovernment.Unknown || NeedsPermit != 0 || Allegiance != EDAllegiance.Unknown ||
                       State != EDState.Unknown || Security != EDSecurity.Unknown || PrimaryEconomy != EDEconomy.Unknown || (Faction != null && Faction.Length > 0);
            }
        }

        public override string ToString()
        {
            return string.Format("{0} @ {1:N1},{2:N1},{3:N1}", Name, X, Y, Z);
        }
    }
}
