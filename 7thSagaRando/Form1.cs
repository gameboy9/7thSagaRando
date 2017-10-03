using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace _7thSagaRando
{
    public partial class Form1 : Form
    {
        byte[] monsterRanking = { 0x24, 0x2f, 0x02, 0x29, 0x30, 0x08, 0x1a, 0x11,
                                      0x0a, 0x40, 0x05, 0x03, 0x0d, 0x07, 0x2c, 0x04,
                                      0x1e, 0x2d, 0x1b, 0x09, 0x3d, 0x43, 0x41, 0x50,
                                      0x15, 0x19, 0x2a, 0x21, 0x1c, 0x06, 0x4e, 0x55,
                                      0x51, 0x0e, 0x2e, 0x0f, 0x16, 0x44, 0x1d, 0x49,
                                      0x3e, 0x45, 0x31, 0x4c, 0x56, 0x57, 0x54, 0x46,
                                      0x17, 0x4d, 0x42, 0x52, 0x4f, 0x47, 0x18, 0x53,
                                      0x48, 0x32, 0x3f, 0x1f, 0x20, 0x58, 0x4a }; // 63 monsters total (in random encounters)
        byte[] legalSpells = { 1, 2, 3, 4, 5, 6, 7,
                                12, 13, 14, 15,
                                16, 17, 21, 22, 23,
                                24, 25, 26, 27, 28, 29, 30, 31,
                                33, 34, 35,
                                40, 41, 45, 46, 47 };

        bool loading = true;
        byte[] romData;
        byte[] romData2;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnNewSeed_Click(object sender, EventArgs e)
        {
            txtSeed.Text = (DateTime.Now.Ticks % 2147483647).ToString();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txtFileName.Text = openFileDialog1.FileName;
                runChecksum();
            }
        }

        private void runChecksum()
        {
            try
            {
                using (var md5 = SHA1.Create())
                {
                    using (var stream = File.OpenRead(txtFileName.Text))
                    {
                        lblSHAChecksum.Text = BitConverter.ToString(md5.ComputeHash(stream)).ToLower().Replace("-", "");
                    }
                }
            }
            catch
            {
                lblSHAChecksum.Text = "????????????????????????????????????????";
            }
        }

        private bool loadRom(bool extra = false)
        {
            try
            {
                romData = File.ReadAllBytes(txtFileName.Text);
                if (extra)
                    romData2 = File.ReadAllBytes(txtCompare.Text);
            }
            catch
            {
                MessageBox.Show("Empty file name(s) or unable to open files.  Please verify the files exist.");
                return false;
            }
            return true;
        }

        private void saveRom()
        {
            string options = "";
            string finalFile = Path.Combine(Path.GetDirectoryName(txtFileName.Text), "7SR_" + txtSeed.Text + "_" + txtFlags.Text + ".smc");
            File.WriteAllBytes(finalFile, romData);
            lblStatus.Text = "ROM hacking complete!  (" + finalFile + ")";
            txtCompare.Text = finalFile;
        }

        private void swap(int firstAddress, int secondAddress)
        {
            byte holdAddress = romData[secondAddress];
            romData[secondAddress] = romData[firstAddress];
            romData[firstAddress] = holdAddress;
        }

        private int[] swapArray(int[] array, int first, int second)
        {
            int holdAddress = array[second];
            array[second] = array[first];
            array[first] = holdAddress;
            return array;
        }

        private int ScaleValue(int value, double scale, double adjustment, Random r1)
        {
            var exponent = (double)r1.Next() / int.MaxValue * 2.0 - 1.0;
            var adjustedScale = 1.0 + adjustment * (scale - 1.0);

            return (int)Math.Round(Math.Pow(adjustedScale, exponent) * value, MidpointRounding.AwayFromZero);
        }

        private int[] inverted_power_curve(int min, int max, int arraySize, double powToUse, Random r1)
        {
            int range = max - min;
            double p_range = Math.Pow(range, 1 / powToUse);
            int[] points = new int[arraySize];
            for (int i = 0; i < arraySize; i++)
            {
                double section = (double)r1.Next() / int.MaxValue;
                points[i] = (int)Math.Round(max - Math.Pow(section * p_range, powToUse));
            }
            Array.Sort(points);
            return points;
        }

        private void determineFlags(object sender, EventArgs e)
        {
            if (loading)
                return;

            string flags = "";
            int number = (chkMonsterZones.Checked ? 1 : 0) + (chkMonsterPatterns.Checked ? 2 : 0) + (chkHeroStats.Checked ? 4 : 0) +
                (chkTreasures.Checked ? 8 : 0) + (chkStores.Checked ? 16 : 0) + (chkWhoCanEquip.Checked ? 32 : 0);
            flags += convertIntToChar(number);
            number = (chkSpeedHacks.Checked ? 1 : 0) + (chkDoubleWalk.Checked ? 2 : 0);
            flags += convertIntToChar(number);
            flags += convertIntToChar(trkExperience.Value);
            flags += convertIntToChar(trkGoldReq.Value - 10);
            flags += convertIntToChar(trkMonsterStats.Value - 10);
            flags += convertIntToChar(trkEquipPowers.Value - 10);
            flags += convertIntToChar(trkSpellCosts.Value - 10);
            flags += convertIntToChar(trkHeroStats.Value - 10);

            txtFlags.Text = flags;
        }

        private void determineChecks(object sender, EventArgs e)
        {
            if (txtFlags.Text.Length != 8) return;
            loading = true;
            string flags = txtFlags.Text;
            int number = convertChartoInt(Convert.ToChar(flags.Substring(0, 1)));
            chkMonsterZones.Checked = (number % 2 == 1);
            chkMonsterPatterns.Checked = (number % 4 >= 2);
            chkHeroStats.Checked = (number % 8 >= 4);
            chkTreasures.Checked = (number % 16 >= 8);
            chkStores.Checked = (number % 32 >= 16);
            chkWhoCanEquip.Checked = (number % 64 >= 32);

            number = convertChartoInt(Convert.ToChar(flags.Substring(1, 1)));
            chkSpeedHacks.Checked = (number % 2 == 1);
            chkDoubleWalk.Checked = (number % 4 >= 2);

            trkExperience.Value = convertChartoInt(Convert.ToChar(flags.Substring(2, 1)));
            trkExperience_Scroll(null, null);
            trkGoldReq.Value = convertChartoInt(Convert.ToChar(flags.Substring(3, 1))) + 10;
            trkGoldReq_Scroll(null, null);
            trkMonsterStats.Value = convertChartoInt(Convert.ToChar(flags.Substring(4, 1))) + 10;
            trkMonsterStats_Scroll(null, null);
            trkEquipPowers.Value = convertChartoInt(Convert.ToChar(flags.Substring(5, 1))) + 10;
            trkEquipPowers_Scroll(null, null);
            trkSpellCosts.Value = convertChartoInt(Convert.ToChar(flags.Substring(6, 1))) + 10;
            trkSpellCosts_Scroll(null, null);
            trkHeroStats.Value = convertChartoInt(Convert.ToChar(flags.Substring(7, 1))) + 10;
            trkHeroStats_Scroll(null, null);
            loading = false;
        }

        private string convertIntToChar(int number)
        {
            if (number >= 0 && number <= 9)
                return number.ToString();
            if (number >= 10 && number <= 35)
                return Convert.ToChar(55 + number).ToString();
            if (number >= 36 && number <= 61)
                return Convert.ToChar(61 + number).ToString();
            if (number == 62) return "!";
            if (number == 63) return "@";
            return "";
        }

        private int convertChartoInt(char character)
        {
            if (character >= Convert.ToChar("0") && character <= Convert.ToChar("9"))
                return character - 48;
            if (character >= Convert.ToChar("A") && character <= Convert.ToChar("Z"))
                return character - 55;
            if (character >= Convert.ToChar("a") && character <= Convert.ToChar("z"))
                return character - 61;
            if (character == Convert.ToChar("!")) return 62;
            if (character == Convert.ToChar("@")) return 63;
            return 0;
        }

        private void trkExperience_Scroll(object sender, EventArgs e)
        {
            lblExperience.Text = (trkExperience.Value * 20).ToString() + "%";
            determineFlags(null, null);
        }

        private void trkGoldReq_Scroll(object sender, EventArgs e)
        {
            lblGoldReq.Text = (trkGoldReq.Value == 10 ? "100%" : (1000 / trkGoldReq.Value) + "-" + (trkGoldReq.Value * 10).ToString() + "%");
            determineFlags(null, null);
        }

        private void trkMonsterStats_Scroll(object sender, EventArgs e)
        {
            lblMonsterStats.Text = (trkMonsterStats.Value == 10 ? "100%" : (1000 / trkMonsterStats.Value) + "-" + (trkMonsterStats.Value * 10).ToString() + "%");
            determineFlags(null, null);
        }

        private void trkEquipPowers_Scroll(object sender, EventArgs e)
        {
            lblEquipPowers.Text = (trkEquipPowers.Value == 10 ? "100%" : (1000 / trkEquipPowers.Value) + "-" + (trkEquipPowers.Value * 10).ToString() + "%");
            determineFlags(null, null);
        }

        private void trkSpellCosts_Scroll(object sender, EventArgs e)
        {
            lblSpellCosts.Text = (trkSpellCosts.Value == 10 ? "100%" : (1000 / trkSpellCosts.Value) + "-" + (trkSpellCosts.Value * 10).ToString() + "%");
            determineFlags(null, null);
        }

        private void trkHeroStats_Scroll(object sender, EventArgs e)
        {
            lblHeroStats.Text = (trkHeroStats.Value == 10 ? "100%" : (1000 / trkHeroStats.Value) + "-" + (trkHeroStats.Value * 10).ToString() + "%");
            determineFlags(null, null);
        }

        private void cmdRandomize_Click(object sender, EventArgs e)
        {
            //try
            {
                loadRom();
                Random r1 = new Random(Convert.ToInt32(txtSeed.Text));
                apprenticeFightAdjustment();
                boostExp();
                if (chkMonsterZones.Checked) randomizeMonsterZones(r1);
                if (chkMonsterPatterns.Checked) randomizeMonsterPatterns(r1);
                if (chkHeroStats.Checked) randomizeHeroStats(r1);
                if (chkTreasures.Checked) randomizeTreasures(r1);
                if (chkStores.Checked) randomizeStores(r1);
                if (chkWhoCanEquip.Checked) randomizeWhoCanEquip(r1);
                if (chkSpeedHacks.Checked) speedHacks();
                if (chkDoubleWalk.Checked) doubleWalk();
                goldRequirements(r1);
                monsterStats(r1);
                heroStats(r1);
                equipmentStats(r1);
                spellCosts(r1);
                saveRom();
            }
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Error:  " + ex.Message);
            //}

            using (StreamWriter writer = File.CreateText(Path.Combine(Path.GetDirectoryName(txtFileName.Text), "7thSaga_" + txtSeed.Text + "_" + txtFlags.Text + "_HeroGuide.txt")))
            {
                for (int lnI = 0; lnI < 7; lnI++)
                {
                    int byteToUse = 0x623f + (18 * lnI);
                    writer.WriteLine(lnI == 0 ? "Kamil" : lnI == 1 ? "Olvan" : lnI == 2 ? "Esuna" : lnI == 3 ? "Wilme" : lnI == 4 ? "Lux" : lnI == 5 ? "Valsu" : "Lejes");
                    writer.WriteLine("Start:   HP:  " + romData[byteToUse] + "  MP:  " + romData[byteToUse + 2] + "  PWR:  " + romData[byteToUse + 4] + "  GRD:  " + romData[byteToUse + 5] + "  MAG:  " + romData[byteToUse + 6] + "  SPD:  " + romData[byteToUse + 7]);
                    writer.WriteLine("Growth:  HP:  " + romData[byteToUse + 8] + "  MP:  " + romData[byteToUse + 9] + "  PWR:  " + romData[byteToUse + 10] + "  GRD:  " + romData[byteToUse + 11] + "  MAG:  " + romData[byteToUse + 12] + "  SPD:  " + romData[byteToUse + 13]);

                    writer.WriteLine("");
                    writer.WriteLine("Weapons: (>= 50 attack)");
                    for (int lnJ = 1; lnJ < 51; lnJ++)
                    {
                        byteToUse = 0x639d + (10 * lnJ);
                        string name = (lnJ == 1 ? "SW Tranq" : lnJ == 2 ? "SW Psyte" : lnJ == 3 ? "SW Anim" : lnJ == 4 ? "SW Kryn" : lnJ == 5 ? "SW Anger" : lnJ == 6 ? "SW Natr" : lnJ == 7 ? "SW Brill" : lnJ == 8 ? "SW Cour" : lnJ == 9 ? "SW Desp" : lnJ == 10 ? "SW Fear" :
                            lnJ == 11 ? "SW Fire" : lnJ == 12 ? "SW Insa" : lnJ == 13 ? "SW Vict" : lnJ == 14 ? "SW Ansc" : lnJ == 15 ? "SW Doom" : lnJ == 16 ? "SW Fort" : lnJ == 19 ? "SW Tidal" : lnJ == 20 ? "SW Znte" :
                            lnJ == 21 ? "SW Mura" : lnJ == 22 ? "KN Lght" : lnJ == 23 ? "SB Saber" : lnJ == 24 ? "KN Fire" : lnJ == 25 ? "Claw" : lnJ == 26 ? "HA Znte" : lnJ == 27 ? "HA Kryn" : lnJ == 28 ? "AX Fire" : lnJ == 29 ? "AX Psyte" : lnJ == 30 ? "AX Anim" :
                            lnJ == 31 ? "AX Anger" : lnJ == 32 ? "AX Power" : lnJ == 33 ? "AX Desp" : lnJ == 34 ? "AX Kryn" : lnJ == 35 ? "AX Fear" : lnJ == 36 ? "AX Myst" : lnJ == 37 ? "AX Hope" : lnJ == 38 ? "AX Immo" : lnJ == 39 ? "SW Sword" :
                            lnJ == 41 ? "ST Lght" : lnJ == 42 ? "ST Petr" : lnJ == 43 ? "RD Tide" : lnJ == 44 ? "RD Conf" : lnJ == 45 ? "RD Brill" : lnJ == 46 ? "RD Desp" : lnJ == 47 ? "RD Natr" : lnJ == 48 ? "RD Fear" : lnJ == 49 ? "RD Myst" : "RD Immo");

                        int power = romData[byteToUse] + (romData[byteToUse + 1] * 256);
                        if (name != "" && romData[byteToUse + 4] % Math.Pow(2, lnI + 1) >= Math.Pow(2, lnI) && power >= 50)
                            writer.WriteLine(name.PadRight(10) + " - " + power.ToString());
                    }

                    writer.WriteLine("");
                    writer.WriteLine("Armor: (>= 40 defense)");
                    for (int lnJ = 0; lnJ < 53; lnJ++)
                    {
                        byteToUse = 0x659b + (17 * lnJ);
                        string name = (lnJ == 0 ? "AR Xtri" : lnJ == 1 ? "AR Psyt" : lnJ == 2 ? "AR Anim" : lnJ == 3 ? "AR Royl" : lnJ == 4 ? "AR Cour" : lnJ == 5 ? "AR Brav" : lnJ == 6 ? "AR Mystc" : lnJ == 7 ? "AR Fort" : 
                            lnJ == 8 ? "ML Scale" : lnJ == 9 ? "ML Chain" : lnJ == 10 ? "ML Kryn" : lnJ == 11 ? "CK Fire" : lnJ == 12 ? "CK Ice" : lnJ == 13 ? "RB Lght" : lnJ == 14 ? "RB Xtre" : lnJ == 15 ? "Xtri" : 
                            lnJ == 16 ? "Coat" : lnJ == 17 ? "Blck" : lnJ == 18 ? "RB Cttn" : lnJ == 19 ? "RB Silk" : lnJ == 20 ? "RB Seas" : lnJ == 21 ? "RB Hope" : lnJ == 22 ? "RB Anger" : lnJ == 23 ? "RB Vict" : 
                            lnJ == 24 ? "RB Desp" : lnJ == 25 ? "RB Conf" : lnJ == 26 ? "RB Myst" : lnJ == 27 ? "RB Immo" : lnJ == 28 ? "Brwn" : lnJ == 30 ? "SH Xtri" : lnJ == 31 ? "SH Kryn" : 
                            lnJ == 32 ? "SH Cour" : lnJ == 33 ? "SH Brill" : lnJ == 34 ? "SH Just" : lnJ == 35 ? "SH Sound" : lnJ == 36 ? "SH Myst" : lnJ == 37 ? "SH Anger" : lnJ == 38 ? "SH Mystc" : lnJ == 39 ? "SH Frtn" :
                            lnJ == 40 ? "SH Immo" : lnJ == 41 ? "SH Illus" : lnJ == 42 ? "Amulet" : lnJ == 43 ? "Ring" : lnJ == 46 ? "Horn" : lnJ == 47 ? "Pod" : 
                            lnJ == 48 ? "HT Xtri" : lnJ == 50 ? "Scarf" : lnJ == 51 ? "MK Mask" : lnJ == 52 ? "MK Kryn" : "CR Brill");

                        int power = romData[byteToUse] + (romData[byteToUse + 1] * 256);
                        if (name != "" && romData[byteToUse + 4] % Math.Pow(2, lnI + 1) >= Math.Pow(2, lnI) && power >= 40)
                            writer.WriteLine(name.PadRight(10) + " - " + power.ToString());
                    }

                    //writer.WriteLine("Spells:");
                    //byteToUse = 0x62bd + (32 * lnI);
                    //for (int lnJ = 0; lnJ < 16; lnJ++)
                    //{
                    //    int spell = romData[byteToUse + lnJ];
                    //    if (spell == 0) break;
                    //    writer.WriteLine(romData[byteToUse + lnJ + 16].ToString() + " - " + (spell == 1 ? "FIRE 1" : spell == 2 ? "FIRE 2" : spell == 3 ? "ICE 1" : spell == 4 ? "ICE 2" : spell == 5 ? "LASER 1" : spell == 6 ? "LASER 2" : spell == 7 ? "LASER 3" : spell == 12 ? "F BIRD" : spell == 13 ? "F BALL" :
                    //        spell == 14 ? "BLZRD 1" : spell == 15 ? "BLZRD 2" : spell == 16 ? "THNDER1" : spell == 17 ? "THNDER2" : spell == 21 ? "PETRIFY" : spell == 22 ? "DEFNSE1" : spell == 23 ? "DEFNSE2" : spell == 24 ? "HEAL 1" : spell == 25 ? "HEAL 2" : spell == 26 ? "HEAL 3" :
                    //        spell == 27 ? "MPCTCHR" : spell == 28 ? "AGILITY" : spell == 29 ? "F SHID" : spell == 30 ? "PROTECT" : spell == 31 ? "EXIT" : spell == 33 ? "POWER" : spell == 34 ? "HPCTCHR" : spell == 35 ? "ELIXIR" : spell == 40 ? "VACUUM1" : spell == 41 ? "VACUUM2" :
                    //        spell == 45 ? "PURIFY" : spell == 46 ? "REVIVE1" : "REVIVE2"));
                    //}
                    //writer.WriteLine("Weapons to equip:");


                    writer.WriteLine("");
                }
            }
        }

        private void apprenticeFightAdjustment()
        {
            //romData[0xbd61] = 0x7f; // Prevent stat boosts when hero level > 10.  That's cheating.
            byte[] romPlugin = { 0x22, 0xd0, 0xf6, 0xc2 };
            for (int lnI = 0; lnI < romPlugin.Length; lnI++)
            {
                romData[0x27872 + lnI] = romPlugin[lnI];
                romData[0x278ad + lnI] = romPlugin[lnI];
                romData[0x278e8 + lnI] = romPlugin[lnI];
                romData[0x27923 + lnI] = romPlugin[lnI];
                romData[0x2795e + lnI] = romPlugin[lnI];
                romData[0x2799e + lnI] = romPlugin[lnI];
            }

            romPlugin = new byte[] { 0xa0, 0x02, 0x00,
                0x22, 0x5b, 0xbd, 0xc0,
                0x6b };

            for (int lnI = 0; lnI < romPlugin.Length; lnI++)
                romData[0x2f6d0 + lnI] = romPlugin[lnI];

            romData[0xca59] = 0xe9; // Force apprentice to be your level MINUS 1 instead of your level PLUS 1.
            romData[0x24852] = 0xea; // None of that doubling of MP either.  Sorry, that's cheating.
        }

        private void speedHacks()
        {
            // Super fast startup screen
            romData[0x506] = 0x01;
            romData[0x52c] = 0x01;
            romData[0xa048e] = 0x01;
            romData[0xa02e1] = 0x00;
            romData[0xa0336] = 0x01;
            romData[0xa0337] = 0x00;
            romData[0xa048e] = 0x01;

            // Menu wraparound (bottom to top - don't work if the menu requires one "screen" only.  I will work on that...)
            byte[] romPlugin = { 0x5c, 0x60, 0xf6, 0xc2 };
            for (int lnI = 0; lnI < romPlugin.Length; lnI++)
                romData[0x474fa + lnI] = romPlugin[lnI];

            romPlugin = new byte[] { 0xb5, 0x37,
                0x18,
                0x75, 0x3d,
                0xd5, 0x35,
                0xb0, 0x04,
                0x5c, 0x03, 0x75, 0xc4,
                0xa9, 0x00,
                0x95, 0x36,
                0x95, 0x37,
                0x5c, 0x07, 0x75, 0xc4 };
            for (int lnI = 0; lnI < romPlugin.Length; lnI++)
                romData[0x2f660 + lnI] = romPlugin[lnI];

            // Menu wraparound (bottom to top - one "screen" only.)
            romPlugin = new byte[] { 0x5c, 0xb0, 0xf6, 0xc2 };
            for (int lnI = 0; lnI < romPlugin.Length; lnI++)
                romData[0x474e3 + lnI] = romPlugin[lnI];

            romPlugin = new byte[] { 0xb5, 0x36,
                0x1a,
                0xd5, 0x35,
                0xb0, 0x04,
                0x5c, 0xea, 0x74, 0xc4,
                0xa9, 0x00,
                0x95, 0x36,
                0x95, 0x37,
                0x5c, 0x07, 0x75, 0xc4 };
            for (int lnI = 0; lnI < romPlugin.Length; lnI++)
                romData[0x2f6b0 + lnI] = romPlugin[lnI];

            // Menu wraparound (top to bottom)
            romPlugin = new byte[] { 0x5c, 0x80, 0xf6, 0xc2 };
            for (int lnI = 0; lnI < romPlugin.Length; lnI++)
                romData[0x4748e + lnI] = romPlugin[lnI];

            romPlugin = new byte[] { 0xb5, 0x37, // LDA $37,x
                0xf0, 0x04, // BEQ $04
                0x5c, 0x92, 0x74, 0xc4, 
                0xb5, 0x35, // LDA $35,x
                0x38, // SEC
                0xf5, 0x3d, // SBC $3D,x
                0x10, 0x09, // BPL
                0xb5, 0x35,
                0x3a,
                0x95, 0x36,
                0x5c, 0x94, 0x74, 0xc4,
                0x95, 0x37,
                0xb5, 0x3d,
                0x3a,
                0x95, 0x36,
                0x5c, 0x94, 0x74, 0xc4 };
            for (int lnI = 0; lnI < romPlugin.Length; lnI++)
                romData[0x2f680 + lnI] = romPlugin[lnI];
        }

        private void doubleWalk()
        {
            // Double walking speed
            for (int lnI = 0x423f7; lnI < 0x42526; lnI++)
            {
                if (romData[lnI] == 0xfe) romData[lnI] = 0xfc;
                if (romData[lnI] == 0x02) romData[lnI] = 0x04;
            }
            romData[0x42095] = 0x08;
        }

        private void randomizeMonsterZones(Random r1)
        {
            byte[] bosses = { 0x0d, 0x0e, 0x1e, 0x21, 0x0f, 0x1f, 0x20, 0x23, 0x27, 0x4a, 0x59 };
            byte[] monsterZoneRanking = { 0x01, 0x02, 0x19, 0x1a, 0x1e, 0x1f, 0x20, 0x21,
                                          0x03, 0x00, 0x04, 0x05, 0x22, 0x23, 0x24, 0x06,
                                          0x07, 0x08, 0x09, 0x25, 0x26, 0x0a, 0x27, 0x28,
                                          0x29, 0x2a, 0x30, 0x31, 0x0b, 0x0c, 0x0d, 0x0e,
                                          0x0f, 0x10, 0x11, 0x2b, 0x2c, 0x2d, 0x2e, 0x2f,
                                          0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x12, 0x13,
                                          0x14, 0x38, 0x39, 0x46, 0x4d, 0x4e, 0x4f, 0x50,
                                          0x58, 0x59, 0x5a }; // 67 zones total

            // Start at 10% chance of help, then increase to a maximum of 60%.
            for (byte lnI = 0; lnI < monsterZoneRanking.Length; lnI++)
            {
                int byteToUse = 0x58df + (monsterZoneRanking[lnI] * 24);
                for (byte lnJ = 0; lnJ < 24; lnJ++)
                    romData[byteToUse + lnJ] = 0x00;
                for (byte lnJ = 0; lnJ < 8; lnJ++)
                {
                    byte minMonster = (byte)(lnI < 46 ? 0 : 10 + ((lnI - 46) * 2));
                    byte maxMonster = (byte)(lnI < 6 ? 5 : lnI < 11 ? 10 : lnI < 18 ? 17 : lnI < 25 ? 26 : 63);
                    byte helpChance = (byte)Math.Min(70, 10 + (lnI * 5));

                    byte monster1 = (byte)(r1.Next() % (maxMonster - minMonster) + minMonster);
                    byte monster2 = 255;
                    byte monster3 = 255;
                    if (r1.Next() % 100 < helpChance && !bosses.Contains(monsterRanking[monster1]))
                    {
                        bool legal = false;
                        while (!legal)
                        {
                            monster2 = (byte)(r1.Next() % (maxMonster - minMonster) + minMonster);
                            if (!bosses.Contains(monsterRanking[monster2])) legal = true;
                        }
                        if (r1.Next() % 100 < helpChance)
                        {
                            legal = false;
                            while (!legal)
                            {
                                monster3 = (byte)(r1.Next() % (maxMonster - minMonster) + minMonster);
                                if (!bosses.Contains(monsterRanking[monster3])) legal = true;
                            }
                        }
                    }
                    romData[byteToUse + lnJ] = monsterRanking[monster1];
                    if (monster2 != 255)
                        romData[byteToUse + lnJ + 8] = monsterRanking[monster2];
                    if (monster3 != 255)
                        romData[byteToUse + lnJ + 16] = monsterRanking[monster3];
                }
            }
        }

        private void randomizeMonsterPatterns(Random r1)
        {
            for (int lnI = 0; lnI < monsterRanking.Length; lnI++)
            {
                int byteToUse = 0x72f4 + (monsterRanking[lnI] * 42);
                if (romData[byteToUse] == 0x46 || romData[byteToUse] == 0x00) continue; // Do not randomize Gorsia or blank monsters.
                for (int lnJ = 0; lnJ < 16; lnJ++)
                    romData[byteToUse + lnJ + 11] = 0x00;
                if (r1.Next() % 2 == 0)
                {
                    int spellTotal = 100;
                    bool duplicate = false;
                    for (int lnJ = 0; lnJ < 7; lnJ++)
                    {
                        romData[byteToUse + lnJ + 11] = legalSpells[r1.Next() % legalSpells.Length];
                        for (int lnK = 0; lnK < lnJ; lnK++)
                            if (romData[byteToUse + lnJ + 11] == romData[byteToUse + lnK + 11]) { romData[byteToUse + lnJ + 11] = 0; duplicate = true; break; }
                        if (duplicate) break;
                        romData[byteToUse + lnJ + 19] = (byte)(r1.Next() % Math.Min(50, spellTotal) + 1);
                        spellTotal -= romData[byteToUse + lnJ + 19];
                        if (spellTotal <= 0) break;
                    }

                    int mp = romData[byteToUse + 3] + (romData[byteToUse + 4] * 256);
                    if (mp < 40) {
                        mp = r1.Next() % 80;
                        romData[byteToUse + 3] = (byte)mp;
                    }
                }
            }

        }

        private void randomizeHeroStats(Random r1)
        {
                for (int lnI = 0; lnI < 7; lnI++)
                {
                    int byteToUse = 0x623f + (18 * lnI);
                    //romData[byteToUse] = (byte)(r1.Next() % 16 + 12); // Starting HP - 12-27
                    //romData[byteToUse + 2] = (byte)(r1.Next() % 21 + 0); // Starting MP - 0-21
                    //romData[byteToUse + 4] = (byte)(r1.Next() % (lnI == 3 || lnI == 4 ? 9 : lnI == 2 || lnI >= 5 ? 8 : 7) + 2); // Starting Power - Kamil/Olvan, 2-8, Esuna/Valsu/Lejes, 2-9, Lux/Wilme, 2-10
                    //romData[byteToUse + 5] = (byte)(r1.Next() % (lnI == 3 || lnI == 4 ? 9 : lnI == 2 || lnI >= 5 ? 8 : 7) + 2); // Starting Guard - Kamil/Olvan, 2-8, Esuna/Valsu/Lejes, 2-9, Lux/Wilme, 2-10
                    //romData[byteToUse + 6] = (byte)(r1.Next() % 7 + 3); // Starting Magic - 3-9
                    //romData[byteToUse + 7] = (byte)(r1.Next() % 7 + 3); // Starting Speed - 3-9
                    //romData[byteToUse + 8] = (byte)(r1.Next() % 7 + 4); // HP Boost - 4-10
                    //romData[byteToUse + 9] = (byte)(r1.Next() % (lnI == 2 || lnI == 5 || lnI == 6 ? 7 : 5) + 2); // MP Boost - Esuna/Valsu/Lejes, 2-8, all others, 2-6
                    //romData[byteToUse + 10] = (byte)(r1.Next() % (lnI == 3 || lnI == 4 ? 7 : lnI == 0 || lnI == 1 ? 6 : 5) + 2); // Power Boost - Lux/Wilme, 2-8, Kamil/Olvan, 2-7, Esuna/Valsu/Lejes, 2-6
                    //romData[byteToUse + 11] = (byte)(r1.Next() % (lnI == 3 || lnI == 4 ? 7 : lnI == 0 || lnI == 1 ? 6 : 5) + 2); // Guard Boost - Lux/Wilme, 2-8, Kamil/Olvan, 2-7, Esuna/Valsu/Lejes, 2-6
                    //romData[byteToUse + 12] = (byte)(r1.Next() % (lnI == 2 || lnI == 5 || lnI == 6 ? 5 : 4) + 2); // Magic Boost - Esuna/Valsu/Lejes, 2-6, all others 2-5
                    //romData[byteToUse + 13] = (byte)(r1.Next() % (lnI == 2 || lnI == 5 || lnI == 6 ? 5 : 4) + 2); // Speed Boost - Esuna/Valsu/Lejes, 2-6, all others 2-5
                    //// 14-16 - Weapon/Armor/Shield
                    //romData[byteToUse + 17] = (byte)(r1.Next() % 100 + 0); // Starting Experience - 0-99

                    List<byte> actualSpells = new List<byte>();
                    // Learn spells as long as you don't duplicate another spell.
                    // Lux/Wilme get one duplicate chance only.  Kamil and Olvan get 10 chances, Lejes gets 50 chances, and Esuna and Valsu get 100 chances. (Lejes gets to equip more stuff than Esuna and Valsu)
                    int duplicateChances = (lnI == 3 || lnI == 4 ? 1 : lnI == 0 || lnI == 1 ? 10 : lnI == 6 ? 50 : 100);
                    for (int lnJ = 0; lnJ < 16; lnJ++)
                    {
                        actualSpells.Add(legalSpells[r1.Next() % legalSpells.Length]);
                        for (int lnK = 0; lnK < lnJ; lnK++)
                            if (actualSpells[lnJ] == actualSpells[lnK]) { duplicateChances--; actualSpells.RemoveAt(actualSpells.Count - 1); lnJ--; break; }
                        if (duplicateChances <= 0) break;
                    }

                    int[] spellLevels = inverted_power_curve(1, 45, actualSpells.Count, 1, r1);

                    byteToUse = 0x62bd + (32 * lnI);
                    for (int lnJ = 0; lnJ < 32; lnJ++)
                        romData[byteToUse + lnJ] = 0;
                    for (int lnJ = 0; lnJ < actualSpells.Count; lnJ++)
                    {
                        romData[byteToUse + lnJ] = actualSpells[lnJ];
                        romData[byteToUse + lnJ + 16] = (byte)spellLevels[lnJ];
                    }
                }
        }

        private void randomizeTreasures(Random r1)
        {
            for (int lnI = 0; lnI < 0xa6; lnI++)
            {
                int byteToUse = 0x8bfd + lnI;
                // Skip if a key treasure is involved because of trigger setting.
                if (romData[byteToUse] >= 0x51 && romData[byteToUse] <= 0x63) continue;
                if (romData[byteToUse] >= 0x03 && romData[byteToUse] <= 0x0a) continue;

                byte[] commonItems = {
                    0x01, 0x02, 0x0b, 0x0c, 0x0d, 0x11, 0x12, 0x13,
                    0x14, 0x29, 0x2d, 0x2e, 0x30, 0x32, 0x34, 0x35,
                    0x38, 0x39, 0x3a, 0x43, 0x44, 0x47, 0x48, 0x49,
                    0x4a
                };
                byte[] rareItems = { 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x4b, 0x4c, 0x4d };
                byte[] weapons = {
                    0x65, 0x66, 0x67, 0x68, 0x69, 0x6a, 0x6b, 0x6c,
                    0x6d, 0x6e, 0x6f, 0x70, 0x71, 0x72, 0x73, 0x74,
                    0x77, 0x78, 0x79, 0x7a, 0x7b, 0x7c, 0x7d, 0x7e,
                    0x7f, 0x80, 0x81, 0x82, 0x83, 0x85, 0x86, 0x87,
                    0x88, 0x89, 0x8a, 0x8b, 0x8d, 0x8e, 0x8f, 0x90,
                    0x91, 0x92, 0x93, 0x94, 0x95, 0x96,
                    0x97, 0x98, 0x99, 0x9a, 0x9b, 0x9c, 0x9d, 0x9e,
                    0x9f, 0xa0, 0xa1, 0xa2, 0xa3, 0xa4, 0xa5, 0xa6,
                    0xa7, 0xa8, 0xa9, 0xaa, 0xab, 0xac, 0xad, 0xae,
                    0xaf, 0xb0, 0xb1, 0xb2, 0xb3, 0xb5, 0xb6, 0xb7,
                    0xb8, 0xb9, 0xba, 0xbb, 0xbc, 0xbd, 0xbe, 0xbf,
                    0xc0, 0xc1, 0xc2, 0xc7, 0xc8, 0xc9, 0xca, 0xcb
                };
                byte[] monsters = { 0xfb, 0xfc, 0xfd, 0xfe };
                byte itemGet = (byte)(r1.Next() % 100);
                if (itemGet < 50)
                    romData[byteToUse] = commonItems[r1.Next() % commonItems.Length];
                else if (itemGet < 75)
                    romData[byteToUse] = rareItems[r1.Next() % rareItems.Length];
                else if (itemGet < 85)
                    romData[byteToUse] = weapons[r1.Next() % weapons.Length];
                else if (itemGet < 95)
                    romData[byteToUse] = monsters[r1.Next() % monsters.Length];
                else
                    romData[byteToUse] = 0x00;
            }
        }

        private void randomizeStores(Random r1)
        {
            byte[] weapons = {
                0x65, 0x66, 0x67, 0x68, 0x69, 0x6a, 0x6b, 0x6c,
                0x6d, 0x6e, 0x6f, 0x70, 0x71, 0x72, 0x73, 0x74,
                0x77, 0x78, 0x79, 0x7a, 0x7b, 0x7c, 0x7d, 0x7e,
                0x7f, 0x80, 0x81, 0x82, 0x83, 0x85, 0x86, 0x87,
                0x88, 0x89, 0x8a, 0x8b, 0x8d, 0x8e, 0x8f, 0x90,
                0x91, 0x92, 0x93, 0x94, 0x95, 0x96
            };
            byte[] armor = {
                0x97, 0x98, 0x99, 0x9a, 0x9b, 0x9c, 0x9d, 0x9e,
                0x9f, 0xa0, 0xa1, 0xa2, 0xa3, 0xa4, 0xa5, 0xa6,
                0xa7, 0xa8, 0xa9, 0xaa, 0xab, 0xac, 0xad, 0xae,
                0xaf, 0xb0, 0xb1, 0xb2, 0xb3, 0xb5, 0xb6, 0xb7,
                0xb8, 0xb9, 0xba, 0xbb, 0xbc, 0xbd, 0xbe, 0xbf,
                0xc0, 0xc1, 0xc2, 0xc7, 0xc8, 0xc9, 0xca, 0xcb
            };

            byte[] items = {
                0x01, 0x02, 0x0b, 0x0c, 0x0d, 0x11, 0x12, 0x13,
                0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b,
                0x29, 0x2d, 0x2e, 0x30, 0x32, 0x34, 0x35, 0x38,
                0x39, 0x3a, 0x43, 0x44, 0x47, 0x48, 0x49, 0x4a,
                0x4b, 0x4c, 0x4d
            };
            for (int lnI = 0; lnI < 40; lnI++)
            {
                List<byte> cityWeapons = new List<byte>();
                int byteToUse = 0x8308 + (lnI * 27);
                // Weapons at bytes 0-4, armor at bytes 5-12, items at bytes 13-21.  I reserve the right to place weapons in armor stores and vice versa.
                for (int lnJ = 0; lnJ < 5; lnJ++)
                {
                    bool duplicate = true;
                    byte currentWeapon = 0;
                    while (duplicate)
                    {
                        currentWeapon = weapons[r1.Next() % weapons.Length];
                        duplicate = false;
                        for (int lnK = 0; lnK < cityWeapons.Count; lnK++)
                            if (currentWeapon == cityWeapons[lnK]) { duplicate = true; break; }
                    }
                    cityWeapons.Sort();
                    cityWeapons.Add(currentWeapon);
                }
                for (int lnJ = 0; lnJ < 5; lnJ++)
                    romData[byteToUse + lnJ] = cityWeapons[lnJ];

                List<byte> cityArmor = new List<byte>();
                for (int lnJ = 6; lnJ < 13; lnJ++)
                {
                    bool duplicate = true;
                    byte currentWeapon = 0;
                    while (duplicate)
                    {
                        currentWeapon = armor[r1.Next() % armor.Length];
                        duplicate = false;
                        for (int lnK = 0; lnK < cityArmor.Count; lnK++)
                            if (currentWeapon == cityArmor[lnK]) { duplicate = true; break; }
                    }
                    cityArmor.Sort();
                    cityArmor.Add(currentWeapon);
                }
                for (int lnJ = 6; lnJ < 13; lnJ++)
                    romData[byteToUse + lnJ] = cityArmor[lnJ - 6];

                List<byte> cityItems = new List<byte>();
                for (int lnJ = 13; lnJ < 22; lnJ++)
                {
                    bool duplicate = true;
                    byte currentItem = 0;
                    while (duplicate)
                    {
                        currentItem = items[r1.Next() % items.Length];
                        duplicate = false;
                        for (int lnK = 0; lnK < cityItems.Count; lnK++)
                        {
                            if (currentItem == cityItems[lnK]) { duplicate = true; break; }
                        }
                    }
                    cityItems.Sort();
                    cityItems.Add(currentItem);
                }
                for (int lnJ = 13; lnJ < 21; lnJ++)
                    romData[byteToUse + lnJ] = cityItems[lnJ - 13];
            }
        }

        private void randomizeWhoCanEquip(Random r1)
        {
            byte[] weapons = {
                1, 2, 3, 4, 5, 6, 7,
                8, 9, 10, 11, 12, 13, 14, 15,
                16, 19, 20, 21, 22, 23,
                24, 25, 26, 27, 28, 29, 30, 31,
                32, 33, 34, 35, 36, 37, 38, 39,
                41, 42, 43, 44, 45, 46, 47,
                48, 49, 50
            };
            byte[] armor = {
                0, 1, 2, 3, 4, 5, 6, 7,
                8, 9, 10, 11, 12, 13, 14, 15,
                16, 17, 18, 19, 20, 21, 22, 23,
                24, 25, 26, 27, 28
            };
            byte[] accessory =
            {
                30, 31,
                32, 33, 34, 35, 36, 37, 38, 39,
                40, 41, 42, 43,
                48, 49, 50, 51, 52
            };

            // Chances of equipping weapon:  Kamil, Olvan - 75%, Lejes - 50%, Esuna, Valsu - 35%, Lux, Wilme - 10%
            // Chances of equipping armor:  Kamil, Olvan - 60%, Esuna, Valsu, Lejes - 50%, Lux, Wilme - 10%
            // Chances of equipping accessory:  Kamil, Olvan - 75%, Esuna, Valsu - 40%, Lejes - 30%, Lux, Wilme - 10%
            // Kamil = 0x01, Olvan = 0x02, Esuna = 0x04, Wilme = 0x08, Lux = 0x10, Valsu = 0x20, Lejes = 0x40 - do not use 0x80.
            for (int lnI = 0; lnI < weapons.Length; lnI++)
            {
                int byteToUse = 0x639d + (10 * weapons[lnI]);
                byte whoEquip = 0;
                if (r1.Next() % 100 < 75) whoEquip += 0x01;
                if (r1.Next() % 100 < 75) whoEquip += 0x02;
                if (r1.Next() % 100 < 35) whoEquip += 0x04;
                if (r1.Next() % 100 < 10) whoEquip += 0x08;
                if (r1.Next() % 100 < 10) whoEquip += 0x10;
                if (r1.Next() % 100 < 35) whoEquip += 0x20;
                if (r1.Next() % 100 < 50) whoEquip += 0x40;
                romData[byteToUse + 4] = whoEquip;
            }
            for (int lnI = 0; lnI < armor.Length; lnI++)
            {
                int byteToUse = 0x659b + (17 * armor[lnI]);
                byte whoEquip = 0;
                if (r1.Next() % 100 < 60) whoEquip += 0x01;
                if (r1.Next() % 100 < 60) whoEquip += 0x02;
                if (r1.Next() % 100 < 50) whoEquip += 0x04;
                if (r1.Next() % 100 < 10) whoEquip += 0x08;
                if (r1.Next() % 100 < 10) whoEquip += 0x10;
                if (r1.Next() % 100 < 50) whoEquip += 0x20;
                if (r1.Next() % 100 < 50) whoEquip += 0x40;
                romData[byteToUse + 4] = whoEquip;
            }
            for (int lnI = 0; lnI < accessory.Length; lnI++)
            {
                int byteToUse = 0x659b + (17 * accessory[lnI]);
                byte whoEquip = 0;
                if (r1.Next() % 100 < 75) whoEquip += 0x01;
                if (r1.Next() % 100 < 75) whoEquip += 0x02;
                if (r1.Next() % 100 < 40) whoEquip += 0x04;
                if (r1.Next() % 100 < 10) whoEquip += 0x08;
                if (r1.Next() % 100 < 10) whoEquip += 0x10;
                if (r1.Next() % 100 < 40) whoEquip += 0x20;
                if (r1.Next() % 100 < 30) whoEquip += 0x40;
                romData[byteToUse + 4] = whoEquip;
            }
        }

        private void boostExp()
        {
            for (int lnI = 0; lnI < 98; lnI++)
            {
                int byteToUse = 0x72f4 + (42 * lnI);
                int xp = romData[byteToUse + 34] + (256 * romData[byteToUse + 35]);
                xp *= (trkExperience.Value * 20 / 100);
                romData[byteToUse + 34] = (byte)(xp % 256);
                romData[byteToUse + 35] = (byte)(xp / 256);
            }
        }

        private void goldRequirements(Random r1)
        {
            // Weapons
            for (int lnI = 0; lnI < 51; lnI++)
            {
                int byteToUse = 0x639d + (10 * lnI);
                statAdjust(r1, byteToUse + 2, 2, trkGoldReq.Value / 10, 1.0);
            }
            // Armor/Accessories
            for (int lnI = 0; lnI < 51; lnI++)
            {
                int byteToUse = 0x659b + (17 * lnI);
                statAdjust(r1, byteToUse + 2, 2, trkGoldReq.Value / 10, 1.0);
            }
            // Items
            for (int lnI = 0; lnI < 100; lnI++)
            {
                int byteToUse = 0x6c94 + (9 * lnI);
                statAdjust(r1, byteToUse + 2, 2, trkGoldReq.Value / 10, 1.0);
            }
            // Inns
            for (int lnI = 0; lnI < 38; lnI++)
            {
                int byteToUse = 0x8308 + (27 * lnI);
                if (romData[byteToUse + 22] == 0) continue;
                statAdjust(r1, byteToUse + 22, 2, trkGoldReq.Value / 10, 1.0);
            }
        }

        private void monsterStats(Random r1)
        {
            for (int lnI = 0; lnI < 90; lnI++)
            {
                int byteToUse = 0x72f4 + (42 * lnI);
                if (romData[byteToUse] == 0x46 || romData[byteToUse] == 0x00) continue; // Do not randomize Gorsia or blank monsters.

                statAdjust(r1, byteToUse + 1, 2, trkMonsterStats.Value / 10, 1.0);
                statAdjust(r1, byteToUse + 3, 2, trkMonsterStats.Value / 10, 1.0);
                statAdjust(r1, byteToUse + 5, 2, trkMonsterStats.Value / 10, 0.5);
                statAdjust(r1, byteToUse + 7, 2, trkMonsterStats.Value / 10, 0.5);
                statAdjust(r1, byteToUse + 9, 1, trkMonsterStats.Value / 10, 0.25);
                statAdjust(r1, byteToUse + 10, 1, trkMonsterStats.Value / 10, 0.25);
                statAdjust(r1, byteToUse + 27, 1, trkMonsterStats.Value / 10, 0.5, 99);
                statAdjust(r1, byteToUse + 28, 1, trkMonsterStats.Value / 10, 0.5, 99);
                statAdjust(r1, byteToUse + 29, 1, trkMonsterStats.Value / 10, 0.5, 99);
                statAdjust(r1, byteToUse + 30, 1, trkMonsterStats.Value / 10, 0.5, 99);
                statAdjust(r1, byteToUse + 31, 1, trkMonsterStats.Value / 10, 0.5, 99);
                statAdjust(r1, byteToUse + 32, 1, trkMonsterStats.Value / 10, 0.5, 99);
                statAdjust(r1, byteToUse + 33, 1, trkMonsterStats.Value / 10, 0.5, 99);
                statAdjust(r1, byteToUse + 34, 2, trkMonsterStats.Value / 10, 1.0);
            }
        }

        private void equipmentStats(Random r1)
        {
            // Weapons
            for (int lnI = 0; lnI < 51; lnI++)
            {
                int byteToUse = 0x639d + (10 * lnI);
                statAdjust(r1, byteToUse, 2, trkEquipPowers.Value / 10, 1.0);
            }
            // Armor/Accessories
            for (int lnI = 0; lnI < 53; lnI++)
            {
                int byteToUse = 0x659b + (17 * lnI);
                statAdjust(r1, byteToUse, 2, trkEquipPowers.Value / 10, 1.0);
            }
        }

        private void spellCosts(Random r1)
        {
            for (int lnI = 0; lnI < 61; lnI++)
            {
                int byteToUse = 0x7018 + (12 * lnI);
                statAdjust(r1, byteToUse + 3, 1, trkSpellCosts.Value / 10, 0.5);
                statAdjust(r1, byteToUse, 2, trkSpellCosts.Value / 10, 1.0);
            }
        }

        private void heroStats(Random r1)
        {
            for (int lnI = 0; lnI < 7; lnI++)
            {
                int byteToUse = 0x623f + (18 * lnI);
                statAdjust(r1, byteToUse, 2, trkHeroStats.Value / 10, 1.0); // Starting HP
                statAdjust(r1, byteToUse + 2, 2, trkHeroStats.Value / 10, 1.0); // Starting MP
                statAdjust(r1, byteToUse + 4, 1, trkHeroStats.Value / 10, 1.0); // Starting Power
                statAdjust(r1, byteToUse + 5, 1, trkHeroStats.Value / 10, 1.0); // Starting Guard
                statAdjust(r1, byteToUse + 6, 1, trkHeroStats.Value / 10, 1.0); // Starting Magic
                statAdjust(r1, byteToUse + 7, 1, trkHeroStats.Value / 10, 1.0); // Starting Speed
                statAdjust(r1, byteToUse + 8, 1, trkHeroStats.Value / 10, 0.5); // HP Boost
                statAdjust(r1, byteToUse + 9, 1, trkHeroStats.Value / 10, 0.5); // MP Boost
                statAdjust(r1, byteToUse + 10, 1, trkHeroStats.Value / 10, 0.5); // Power Boost
                statAdjust(r1, byteToUse + 11, 1, trkHeroStats.Value / 10, 0.5); // Guard Boost
                statAdjust(r1, byteToUse + 12, 1, trkHeroStats.Value / 10, 0.5); // Magic Boost
                statAdjust(r1, byteToUse + 13, 1, trkHeroStats.Value / 10, 0.5); // Speed Boost
                statAdjust(r1, byteToUse + 17, 1, trkHeroStats.Value / 10, 1.0); // Starting Experience
            }
        }

        private void statAdjust(Random r1, int byteToUse, int bytes, double scale, double adjustment, int max = 0)
        {
            if (max == 0) max = (bytes == 2 ? 65500 : 255);
            if (bytes == 2)
            {
                int stat = romData[byteToUse] + (256 * romData[byteToUse + 1]);
                if (stat != 0)
                    stat = ScaleValue(stat, scale, adjustment, r1);
                if (stat > max) stat = max;
                romData[byteToUse] = (byte)(stat % 256);
                romData[byteToUse + 1] = (byte)(stat / 256);
            } else
            {
                int stat = romData[byteToUse];
                if (stat != 0)
                    stat = ScaleValue(stat, scale, adjustment, r1);
                if (stat > max) stat = max;
                romData[byteToUse] = (byte)(stat);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtSeed.Text = (DateTime.Now.Ticks % 2147483647).ToString();

            try
            {
                using (TextReader reader = File.OpenText("last7th.txt"))
                {
                    txtFlags.Text = reader.ReadLine();
                    txtFileName.Text = reader.ReadLine();

                    determineChecks(null, null);

                    runChecksum();
                    loading = false;
                }
            }
            catch
            {
                // ignore error
                loading = false;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            using (StreamWriter writer = File.CreateText("last7th.txt"))
            {
                writer.WriteLine(txtFlags.Text);
                writer.WriteLine(txtFileName.Text);
            }
        }
    }
}
