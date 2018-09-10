/*
   Factorio server info receiving source
    Copyright (C) 2018 Roman Svistunov

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
	*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace getserverinfo
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string[] ipparts = args[0].Split(':');
                IPAddress ip = IPAddress.Parse(ipparts[0]);
                int port = int.Parse(ipparts[1]);

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.ReceiveTimeout = 1000;

                IPEndPoint server = new IPEndPoint(ip, port);
                var sinf = GetServerInfo(server, socket);
                StreamWriter sw = new StreamWriter(ipparts[0] + "_" + ipparts[1] + ".sinf");
                sw.Write(sinf);
                sw.Close();
            }
            catch(Exception ex)
            {
                StreamWriter sw = new StreamWriter(new FileStream("gsi.log", FileMode.Append, FileAccess.Write));
                sw.WriteLine(ex);
                sw.Close();
            }
        }
        static ServerInfo GetServerInfo(IPEndPoint server, Socket socket)
        {
            List<Tuple<byte[], int>> __tmp;
            try
            {
                socket.Connect(server);
                byte[] packet0 = new byte[] { 0x22, 0x00, 0x00, 0x00, 0x10, 0x33, 0x2e, 0x8f, 0x00, 0x01, 0x02, 0x03 };
                socket.Send(packet0);
                byte[] ans0 = new byte[256];
                socket.Receive(ans0);
                byte[] packet1 = new byte[] { 0x24, 0x01, 0x80, 0x01, ans0[1], 0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0x03, ans0[17], ans0[18], ans0[19], ans0[20], 0x00, 0x01, 0x02, 0x03, 0x03, 0x62, 0x6f, 0x74, 0x00, 0x00, 0x00, 0x42, 0x40, 0x16, 0x32, 0x01, 0x04, 0x62, 0x61, 0x73, 0x65, 0x00, 0x10, 0x33, 0xa6, 0x83, 0x14, 0xc6, 0x00 };
                socket.Send(packet1);
                __tmp = ReceiveFullHeader(socket);
            }
            catch(SocketException)
            {
                return new ServerInfo(false, null, null, null, null);
            }
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            return ParsePacket(MergePackets(__tmp));
        }
        static List<Tuple<byte[], int>> ReceiveFullHeader(Socket socket)
        {
            List<Tuple<byte[], int>> tuples = new List<Tuple<byte[], int>>();
            try
            {
                byte tockeck = 0;
                for (; ; )
                {
                    byte[] ans = new byte[1000];
                    int ln = socket.Receive(ans);
                    if (IndexOf(ans, new byte[] { 0, 1, 2, 3 }) >= 0 && tockeck == 0) tockeck = ans[1];
                    if (ans[1] != tockeck && tockeck != 0) break;
                    tuples.Add(Tuple.Create(ans, ln));
                }
            }
            catch (SocketException)
            {

            }
            return tuples;
        }
        static byte[] MergePackets(List<Tuple<byte[], int>> tuples)
        {
            byte[] firstcheck = new byte[] { 0x00, 0x01, 0x02, 0x03 };
            var real = tuples.Find(i => IndexOf(i.Item1, firstcheck) >= 0).Item1;
            return (from i in tuples let a = i.Item1 where a[1] == real[1] orderby a[3] ascending group a.Skip(4).Take(i.Item2 - 4) by a[3]).Select(i => i.First()).Aggregate((a, b) => a.Concat(b)).ToArray();
        }
        static ServerInfo ParsePacket(byte[] data)
        {
            int position = 0;

            GoToServerName(data, ref position);
            string servername = Encoding.ASCII.GetString(data, position + 2, data[position + 1]);
            position += data[position + 1] + 2;

            GoToAdminsList(data, ref position);
            List<string> admins = new List<string>();
            bool stop = false;
            do
            {
                admins.Add(GetAdmin(data, ref position, out stop));
            } while (!stop);

            List<string> players = new List<string>();
            int playerscnt = data[position];
            for(int ijk = 0; ijk < playerscnt; ijk++)
                players.Add(GetPlayer(data, ref position, out stop));

            position += 11;
            byte modscnt = data[position++];
            List<ModInfo> mods = new List<ModInfo>();
            for(int ijk = 0; ijk < modscnt; ijk++)
            {
                mods.Add(GetMod(data, ref position));
                position += 4;
            }

            return new ServerInfo(true, servername, admins, players, mods);
        }
        static void GoToServerName(byte[] data, ref int position)
        {
            int namemaxachieve = 0;
            while (namemaxachieve < 4)
            {
                byte val = data[position++];
                if (val == namemaxachieve) namemaxachieve++;
                else
                {
                    if (val == 0) namemaxachieve = 1;
                    else namemaxachieve = 0;
                }
            }
        }
        static void GoToAdminsList(byte[] data, ref int position)
        {
            while (!(data[position - 2] == 0x00 && data[position - 1] == 0x20))
            {
                position++;
            }
            position += 2;
        }
        static string GetAdmin(byte[] data, ref int position, out bool isfinished)
        {
            position += 2;
            string player = Encoding.ASCII.GetString(data, position + 1, data[position]);
            position += data[position] + 1;
            if (data[position] == 0xff && data[position + 1] == 0x00)
            {
                position += 2;
                isfinished = true;
                return player;
            }
            isfinished = false;
            return player;
        }
        static string GetPlayer(byte[] data, ref int position, out bool isfinished)
        {
            position += 2;
            string player = Encoding.ASCII.GetString(data, position + 1, data[position]);
            position += data[position] + 1;
            isfinished = data[position] == 0x00;
            return player;
        }
        static ModInfo GetMod(byte[] data, ref int position)
        {
            string mod_name = Encoding.ASCII.GetString(data, position + 1, data[position]);
            int superpos = 0;
            position += data[position] + 1;

            int major_version = data[position++];
            while(data[position - 1] == 0xff)
            {
                major_version += data[position++];
                superpos++;
            }

            int minor_version = data[position++];
            while (data[position - 1] == 0xff)
            {
                minor_version += data[position++];
                superpos++;
            }

            int assembly_number = data[position++];
            while (data[position - 1] == 0xff)
            {
                assembly_number += data[position++];
                superpos++;
            }
            position += superpos;
            return new ModInfo(mod_name, major_version, minor_version, assembly_number);
        }
        static int IndexOf(byte[] array, byte[] other)
        {
            for(int i = 0; i < array.Length - other.Length; i++)
            {
                bool ok = true;
                for(int j = 0; j < other.Length; j++)
                {
                    if(array[i+j] != other[j])
                    {
                        ok = false;
                        break;
                    }
                }
                if(ok)
                {
                    return i;
                }
            }
            return -1;
        }
    }
    struct ServerInfo
    {
        public bool state;
        public string name;
        public List<string> admins;
        public List<string> players;
        public List<ModInfo> mods;
        public ServerInfo(bool state, string name, List<string> admins, List<string> players, List<ModInfo> mods)
        {
            this.state = state;
            this.name = name;
            this.admins = admins;
            this.players = players;
            this.mods = mods;
        }
        public override string ToString()
        {
            string ret = state ? "ONLINE" + Environment.NewLine : "OFFLINE" + Environment.NewLine;
            if (!state) return ret;

            ret += (name + Environment.NewLine);

            ret += (admins.Count + Environment.NewLine);
            foreach (var admin in admins)
            {
                ret += (admin + Environment.NewLine);
            }

            ret += (players.Count + Environment.NewLine);
            foreach (var player in players)
            {
                ret += (player + Environment.NewLine);
            }

            ret += (mods.Count + Environment.NewLine);
            foreach(ModInfo mod in mods)
            {
                ret += (mod.ToString() + Environment.NewLine);
            }

            return ret;
        }
    }
    struct ModInfo
    {
        string name;
        int MajorVersion;
        int MinorVersion;
        int AssemblyVersion;

        public ModInfo(string name, int majorVersion, int minorVersion, int assemblyVersion)
        {
            this.name = name;
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
            AssemblyVersion = assemblyVersion;
        }

        public override string ToString()
        {
            return name + " " + MajorVersion + "." + MinorVersion + "." + AssemblyVersion;
        }
    }
}
