﻿using System;
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
        byte[] monsterLegalSpells = { 1, 2, 3, 4, 5, 6, 7,
                                12, 13, 14, 15,
                                16, 17, 21, 22, 23,
                                24, 25, 26, 27, 28, 
                                33, 34,
                                40, 41, 46, 47, 55 };
        byte[] legalSpells = { 1, 2, 3, 4, 5, 6, 7,
                                12, 13, 14, 15,
                                16, 17, 21, 22, 23,
                                24, 25, 26, 27, 28, 29, 30, 31,
                                32, 33, 34, 35,
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

        private int ScaleValue(double value, double scale, double adjustment, Random r1, bool min100 = false)
        {
            double exponent = (double)r1.Next() / int.MaxValue;
            exponent = (min100 ? exponent : exponent * 2.0 - 1.0);
            var adjustedScale = 1.0 + adjustment * (scale - 1.0);

            return (int)Math.Round(Math.Pow(adjustedScale, exponent) * value, MidpointRounding.AwayFromZero);
        }

        private double ScaleValueDouble(double value, double scale, double adjustment, Random r1, bool min100 = false)
        {
            double exponent = (double)r1.Next() / int.MaxValue;
            exponent = (min100 ? exponent : exponent * 2.0 - 1.0);
            var adjustedScale = 1.0 + adjustment * (scale - 1.0);

            return Math.Pow(adjustedScale, exponent) * value;
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
            int number = (chkMonsterZones.Checked ? 1 : 0) + (chkMonsterPatterns.Checked ? 2 : 0) + (chkHeroSpells.Checked ? 4 : 0) +
                (chkTreasures.Checked ? 8 : 0) + (chkStores.Checked ? 16 : 0) + (chkWhoCanEquip.Checked ? 32 : 0);
            flags += convertIntToChar(number);
            number = (chkSpeedHacks.Checked ? 1 : 0) + (chkDoubleWalk.Checked ? 2 : 0) + (chkShowStatGains.Checked ? 4 : 0) + 
                (chkNoXPGPRando.Checked ? 8 : 0) + (chkHeroSameStats.Checked ? 16 : 0) + (chkHeroSameSpell.Checked ? 32 : 0);
            flags += convertIntToChar(number);
            number = (chkHeroSameEquip.Checked ? 1 : 0) + (chkMonsterDrops.Checked ? 2 : 0) + (chkSameRando.Checked ? 4 : 0) +
                (chkRemoveTriggers.Checked ? 8 : 0) + (chkNoSeeds.Checked ? 16 : 0) + (chkNoStoreItems.Checked ? 32 : 0);
            flags += convertIntToChar(number);
            number = (chkHeroInteractions.Checked ? 1 : 0) + (chkAllHeroesJoin.Checked ? 2 : 0) + (chkNoHeroesJoin.Checked ? 4 : 0) +
                (chkSpellLearning.Checked ? 8 : 0) + (chkLearnNoMagic.Checked ? 16 : 0) + (chk9999Defense.Checked ? 32 : 0);
            flags += convertIntToChar(number);
            number = (chkWindRune1.Checked ? 1 : 0) + (chkWindRune2.Checked ? 2 : 0) + (chkWindRune3.Checked ? 4 : 0) +
                (chkWindRune4.Checked ? 8 : 0) + (chkWindRune5.Checked ? 16 : 0) + (chkDebuffBoss.Checked ? 32 : 0);
            flags += convertIntToChar(number);
            number = (chkGoldMin.Checked ? 1 : 0) + (chkHeroStatMin.Checked ? 2 : 0) + (chkMonsterStatMin.Checked ? 4 : 0) +
                (chkEquipMin.Checked ? 8 : 0) + (chkSpellMin.Checked ? 16 : 0) + (chkHeroGrowthMin.Checked ? 32 : 0);
            flags += convertIntToChar(number);
            number = (chkPostBoneRandom.Checked ? 1 : 0) + (chkPostBonePandam.Checked ? 2 : 0) + (chkPostBoneRemote.Checked ? 4 : 0) +
                (chkPostBoneGrime.Checked ? 8 : 0) + (chkFullXP.Checked ? 16 : 0) + (chkVacuumBoss.Checked ? 32 : 0);
            flags += convertIntToChar(number);
            number = (chkBrushAirship1.Checked ? 1 : 0) + (chkBrushAirship2.Checked ? 2 : 0);
            flags += convertIntToChar(number);
            flags += convertIntToChar(trkExperience.Value);
            flags += convertIntToChar(trkGoldReq.Value - 10);
            flags += convertIntToChar(trkMonsterStats.Value - 10);
            flags += convertIntToChar(trkEquipPowers.Value - 10);
            flags += convertIntToChar(trkSpellCosts.Value - 10);
            flags += convertIntToChar(trkHeroStats.Value - 10);
            flags += convertIntToChar(trkHeroGrowth.Value - 10);

            txtFlags.Text = flags;
        }

        private void determineChecks(object sender, EventArgs e)
        {
            if (txtFlags.Text.Length != 15) return;
            loading = true;
            string flags = txtFlags.Text;
            int number = convertChartoInt(Convert.ToChar(flags.Substring(0, 1)));
            chkMonsterZones.Checked = (number % 2 == 1);
            chkMonsterPatterns.Checked = (number % 4 >= 2);
            chkHeroSpells.Checked = (number % 8 >= 4);
            chkTreasures.Checked = (number % 16 >= 8);
            chkStores.Checked = (number % 32 >= 16);
            chkWhoCanEquip.Checked = (number % 64 >= 32);

            number = convertChartoInt(Convert.ToChar(flags.Substring(1, 1)));
            chkSpeedHacks.Checked = (number % 2 == 1);
            chkDoubleWalk.Checked = (number % 4 >= 2);
            chkShowStatGains.Checked = (number % 8 >= 4);
            chkNoXPGPRando.Checked = (number % 16 >= 8);
            chkHeroSameStats.Checked = (number % 32 >= 16);
            chkHeroSameSpell.Checked = (number % 64 >= 32);

            number = convertChartoInt(Convert.ToChar(flags.Substring(2, 1)));
            chkHeroSameEquip.Checked = (number % 2 == 1);
            chkMonsterDrops.Checked = (number % 4 >= 2);
            chkSameRando.Checked = (number % 8 >= 4);
            chkRemoveTriggers.Checked = (number % 16 >= 8);
            chkNoSeeds.Checked = (number % 32 >= 16);
            chkNoStoreItems.Checked = (number % 64 >= 32);

            number = convertChartoInt(Convert.ToChar(flags.Substring(3, 1)));
            chkHeroInteractions.Checked = (number % 2 == 1);
            chkAllHeroesJoin.Checked = (number % 4 >= 2);
            chkNoHeroesJoin.Checked = (number % 8 >= 4);
            chkSpellLearning.Checked = (number % 16 >= 8);
            chkLearnNoMagic.Checked = (number % 32 >= 16);
            chk9999Defense.Checked = (number % 64 >= 32);

            number = convertChartoInt(Convert.ToChar(flags.Substring(4, 1)));
            chkWindRune1.Checked = (number % 2 == 1);
            chkWindRune2.Checked = (number % 4 >= 2);
            chkWindRune3.Checked = (number % 8 >= 4);
            chkWindRune4.Checked = (number % 16 >= 8);
            chkWindRune5.Checked = (number % 32 >= 16);
            chkDebuffBoss.Checked = (number % 64 >= 32);

            number = convertChartoInt(Convert.ToChar(flags.Substring(5, 1)));
            chkGoldMin.Checked = (number % 2 == 1);
            chkHeroStatMin.Checked = (number % 4 >= 2);
            chkMonsterStatMin.Checked = (number % 8 >= 4);
            chkEquipMin.Checked = (number % 16 >= 8);
            chkSpellMin.Checked = (number % 32 >= 16);
            chkHeroGrowthMin.Checked = (number % 64 >= 32);

            number = convertChartoInt(Convert.ToChar(flags.Substring(6, 1)));
            chkPostBoneRandom.Checked = (number % 2 == 1);
            chkPostBonePandam.Checked = (number % 4 >= 2);
            chkPostBoneRemote.Checked = (number % 8 >= 4);
            chkPostBoneGrime.Checked = (number % 16 >= 8);
            chkFullXP.Checked = (number % 32 >= 16);
            chkVacuumBoss.Checked = (number % 64 >= 32);

            number = convertChartoInt(Convert.ToChar(flags.Substring(7, 1)));
            chkBrushAirship1.Checked = (number % 2 == 1);
            chkBrushAirship2.Checked = (number % 4 >= 2);

            trkExperience.Value = convertChartoInt(Convert.ToChar(flags.Substring(8, 1)));
            trkExperience_Scroll(null, null);
            trkGoldReq.Value = convertChartoInt(Convert.ToChar(flags.Substring(9, 1))) + 10;
            trkGoldReq_Scroll(null, null);
            trkMonsterStats.Value = convertChartoInt(Convert.ToChar(flags.Substring(10, 1))) + 10;
            trkMonsterStats_Scroll(null, null);
            trkEquipPowers.Value = convertChartoInt(Convert.ToChar(flags.Substring(11, 1))) + 10;
            trkEquipPowers_Scroll(null, null);
            trkSpellCosts.Value = convertChartoInt(Convert.ToChar(flags.Substring(12, 1))) + 10;
            trkSpellCosts_Scroll(null, null);
            trkHeroStats.Value = convertChartoInt(Convert.ToChar(flags.Substring(13, 1))) + 10;
            trkHeroStats_Scroll(null, null);
            trkHeroGrowth.Value = convertChartoInt(Convert.ToChar(flags.Substring(14, 1))) + 10;
            trkHeroGrowth_Scroll(null, null);
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
            lblGoldReq.Text = (trkGoldReq.Value == 10 ? "100%" : (chkGoldMin.Checked ? 100 : (1000 / trkGoldReq.Value)) + "-" + (trkGoldReq.Value * 10).ToString() + "%");
            determineFlags(null, null);
        }

        private void trkMonsterStats_Scroll(object sender, EventArgs e)
        {
            lblMonsterStats.Text = (trkMonsterStats.Value == 10 ? "100%" : (chkMonsterStatMin.Checked ? 100 : (1000 / trkMonsterStats.Value)) + "-" + (trkMonsterStats.Value * 10).ToString() + "%");
            determineFlags(null, null);
        }

        private void trkEquipPowers_Scroll(object sender, EventArgs e)
        {
            lblEquipPowers.Text = (trkEquipPowers.Value == 10 ? "100%" : (chkEquipMin.Checked ? 100 : (1000 / trkEquipPowers.Value)) + "-" + (trkEquipPowers.Value * 10).ToString() + "%");
            determineFlags(null, null);
        }

        private void trkSpellCosts_Scroll(object sender, EventArgs e)
        {
            lblSpellCosts.Text = (trkSpellCosts.Value == 10 ? "100%" : (chkSpellMin.Checked ? 100 : (1000 / trkSpellCosts.Value)) + "-" + (trkSpellCosts.Value * 10).ToString() + "%");
            determineFlags(null, null);
        }

        private void trkHeroStats_Scroll(object sender, EventArgs e)
        {
            lblHeroStats.Text = (trkHeroStats.Value == 10 ? "100%" : (chkHeroStatMin.Checked ? 100 : (1000 / trkHeroStats.Value)) + "-" + (trkHeroStats.Value * 10).ToString() + "%");
            determineFlags(null, null);
        }

        private void trkHeroGrowth_Scroll(object sender, EventArgs e)
        {
            lblHeroGrowth.Text = (trkHeroGrowth.Value == 10 ? "100%" : (chkHeroGrowthMin.Checked ? 100 : (1000 / trkHeroGrowth.Value)) + "-" + (trkHeroGrowth.Value * 10).ToString() + "%");
            determineFlags(null, null);
        }

        private void randomize()
        {
            loadRom();
            Random r1 = new Random(Convert.ToInt32(txtSeed.Text));
            apprenticeFightAdjustment(r1);
            boostExp();
            if (chkMonsterZones.Checked) randomizeMonsterZones(r1);
            if (chkMonsterPatterns.Checked) randomizeMonsterPatterns(r1);
            if (chkMonsterDrops.Checked) randomizeMonsterDrops(r1);
            if (chkHeroSpells.Checked || chkSpellLearning.Checked || chkLearnNoMagic.Checked) randomizeHeroSpells(r1);
            if (chkTreasures.Checked) randomizeTreasures(r1);
            if (chkStores.Checked || chkNoSeeds.Checked) randomizeStores(r1);
            if (chkNoStoreItems.Checked) noStoreItems();
            if (chkWhoCanEquip.Checked) randomizeWhoCanEquip(r1);
            if (chkFullXP.Checked) noXPSplitting();
            heroInteractions(r1);
            randomizePison(r1);
            randomizePandam(r1);
            goldRequirements(r1);
            monsterStats(r1);
            heroStats(r1);
            equipmentStats(r1);
            spellCosts(r1);
            if (chkSpeedHacks.Checked) speedHacks();

            // Remove stat gain text on level up.
            if (!chkShowStatGains.Checked) romData[0x18cd1] = 0x6b;
            // Enable debuffs and vacuums on bosses by replacing BEQ with BRA
            if (chkDebuffBoss.Checked) romData[0x4bf6a] = 0x80;
            if (chkVacuumBoss.Checked) romData[0x4be60] = 0x80;

            // Speedup airship ride
            if (chkBrushAirship1.Checked)
            {
                romData[0x39a5] = 0x60;
                romData[0x39a6] = 0x07;
                if (chkBrushAirship2.Checked)
                {
                    for (int lnI = 0x500e8; lnI <= 0x50228; lnI += 8)
                        romData[lnI] = 0x01;
                    romData[0x39a5] = 0xb0;
                    romData[0x39a6] = 0x00;
                }
            }

            adjustExperienceTable();

            if (chkDoubleWalk.Checked) doubleWalk();
            if (chkRemoveTriggers.Checked) removeUselessTriggers();
            if (chkWindRune1.Checked || chkWindRune2.Checked || chkWindRune3.Checked || chkWindRune4.Checked || chkWindRune5.Checked) freeIce();
        }

        private void adjustExperienceTable()
        {
            romData[0x28045] = 0x00;
            romData[0x28046] = 0x01;

            romData[0x2800a] = 0x74;
            romData[0x2800b] = 0x01;

            for (int lnI = 0; lnI < 80; lnI++)
            {
                int byteToUse = 0x8cc8 + (3 * lnI);
                double xp = romData[byteToUse + 0] + (romData[byteToUse + 1] * 256) + (romData[byteToUse + 2] * 65536);
                xp /= 2.203125;
                int newXP = (int)Math.Round(xp);

                romData[byteToUse + 0] = (byte)(newXP % 256);
                romData[byteToUse + 1] = (byte)((newXP / 256) % 256);
                romData[byteToUse + 2] = (byte)(newXP / 65536);
            }
        }

        private void cmdRandomize_Click(object sender, EventArgs e)
        {
            try
            {
                randomize();
                saveRom();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:  " + ex.Message);
            }
        }

        private void heroInteractions(Random r1)
        {
            if (chkHeroInteractions.Checked)
            {
                for (int lnI = 0; lnI < 7; lnI++)
                {
                    int byteToUse = 0x8740 + (60 * lnI);
                    for (int lnJ = 0; lnJ < 7; lnJ++)
                    {
                        if (lnJ == lnI) continue;
                        byteToUse = 0x8740 + (60 * lnI) + 18 + (6 * lnJ);
                        int[] chances = { r1.Next() % 100, r1.Next() % 100, r1.Next() % 100, r1.Next() % 100, r1.Next() % 100, r1.Next() % 100 };
                        int chanceTotal = chances[0] + chances[1] + chances[2] + chances[3] + chances[4] + chances[5];
                        int finalTotal = 100;
                        for (int lnK = 0; lnK < 5; lnK++)
                        {
                            romData[byteToUse + lnK] = (byte)(chances[lnK] * 100 / chanceTotal);
                            finalTotal -= romData[byteToUse + lnK];
                        }
                        if (finalTotal < 0) finalTotal = 0;
                        romData[byteToUse + 5] = (byte)finalTotal;
                    }
                }
            } else if (chkAllHeroesJoin.Checked)
            {
                for (int lnI = 0; lnI < 7; lnI++)
                {
                    int byteToUse = 0x8740 + (60 * lnI);
                    for (int lnJ = 0; lnJ < 7; lnJ++)
                    {
                        if (lnJ == lnI) continue;
                        byteToUse = 0x8740 + (60 * lnI) + 18 + (6 * lnJ);
                        romData[byteToUse + 0] = 50;
                        romData[byteToUse + 1] = 50;
                        romData[byteToUse + 2] = 0;
                        romData[byteToUse + 3] = 0;
                        romData[byteToUse + 4] = 0;
                        romData[byteToUse + 5] = 0;
                    }
                }
            }
            else if (chkNoHeroesJoin.Checked)
            {
                for (int lnI = 0; lnI < 7; lnI++)
                {
                    int byteToUse = 0x8740 + (60 * lnI);
                    for (int lnJ = 0; lnJ < 7; lnJ++)
                    {
                        if (lnJ == lnI) continue;
                        byteToUse = 0x8740 + (60 * lnI) + 18 + (6 * lnJ);
                        romData[byteToUse + 0] = 0;
                        romData[byteToUse + 1] = 0;
                        romData[byteToUse + 2] = 25;
                        romData[byteToUse + 3] = 25;
                        romData[byteToUse + 4] = 25;
                        romData[byteToUse + 5] = 25;
                    }
                }
            }
        }

        private StreamWriter printStats(StreamWriter writer, int byteToUse, string starter)
        {
            writer.WriteLine(starter.PadRight(20) + romData[byteToUse].ToString().PadLeft(10) + romData[byteToUse + 18].ToString().PadLeft(10) + romData[byteToUse + 36].ToString().PadLeft(10) + romData[byteToUse + 54].ToString().PadLeft(10) + 
                romData[byteToUse + 72].ToString().PadLeft(10) + romData[byteToUse + 90].ToString().PadLeft(10) + romData[byteToUse + 108].ToString().PadLeft(10));
            return writer;
        }

        private StreamWriter printWeapons(StreamWriter writer, int byteToUse, int lnJ)
        {
            string name = (lnJ == 1 ? "SW Tranq" : lnJ == 2 ? "SW Psyte" : lnJ == 3 ? "SW Anim" : lnJ == 4 ? "SW Kryn" : lnJ == 5 ? "SW Anger" : lnJ == 6 ? "SW Natr" : lnJ == 7 ? "SW Brill" : lnJ == 8 ? "SW Cour" : lnJ == 9 ? "SW Desp" : lnJ == 10 ? "SW Fear" :
                lnJ == 11 ? "SW Fire" : lnJ == 12 ? "SW Insa" : lnJ == 13 ? "SW Vict" : lnJ == 14 ? "SW Ansc" : lnJ == 15 ? "SW Doom" : lnJ == 16 ? "SW Fort" : lnJ == 19 ? "SW Tidal" : lnJ == 20 ? "SW Znte" :
                lnJ == 21 ? "SW Mura" : lnJ == 22 ? "KN Lght" : lnJ == 23 ? "SB Saber" : lnJ == 24 ? "KN Fire" : lnJ == 25 ? "Claw" : lnJ == 26 ? "HA Znte" : lnJ == 27 ? "HA Kryn" : lnJ == 28 ? "AX Fire" : lnJ == 29 ? "AX Psyte" : lnJ == 30 ? "AX Anim" :
                lnJ == 31 ? "AX Anger" : lnJ == 32 ? "AX Power" : lnJ == 33 ? "AX Desp" : lnJ == 34 ? "AX Kryn" : lnJ == 35 ? "AX Fear" : lnJ == 36 ? "AX Myst" : lnJ == 37 ? "AX Hope" : lnJ == 38 ? "AX Immo" : lnJ == 39 ? "SW Sword" :
                lnJ == 41 ? "ST Lght" : lnJ == 42 ? "ST Petr" : lnJ == 43 ? "RD Tide" : lnJ == 44 ? "RD Conf" : lnJ == 45 ? "RD Brill" : lnJ == 46 ? "RD Desp" : lnJ == 47 ? "RD Natr" : lnJ == 48 ? "RD Fear" : lnJ == 49 ? "RD Myst" : "RD Immo");

            if (name != "")
            {
                string printOut = name.PadRight(20);
                for (int lnI = 0; lnI < 7; lnI++)
                {
                    if (romData[byteToUse + 4] % Math.Pow(2, lnI + 1) >= Math.Pow(2, lnI))
                        printOut += (romData[byteToUse] + (romData[byteToUse + 1] * 256)).ToString().PadLeft(10);
                    else
                        printOut += "---".PadLeft(10);
                }

                writer.WriteLine(printOut);
            }

            return writer;
        }

        private StreamWriter printArmor(StreamWriter writer, int byteToUse, int lnJ)
        {
            string name = (lnJ == 0 ? "AR Xtri" : lnJ == 1 ? "AR Psyt" : lnJ == 2 ? "AR Anim" : lnJ == 3 ? "AR Royl" : lnJ == 4 ? "AR Cour" : lnJ == 5 ? "AR Brav" : lnJ == 6 ? "AR Mystc" : lnJ == 7 ? "AR Fort" :
                lnJ == 8 ? "ML Scale" : lnJ == 9 ? "ML Chain" : lnJ == 10 ? "ML Kryn" : lnJ == 11 ? "CK Fire" : lnJ == 12 ? "CK Ice" : lnJ == 13 ? "RB Lght" : lnJ == 14 ? "RB Xtre" : lnJ == 15 ? "Xtri" :
                lnJ == 16 ? "Coat" : lnJ == 17 ? "Blck" : lnJ == 18 ? "RB Cttn" : lnJ == 19 ? "RB Silk" : lnJ == 20 ? "RB Seas" : lnJ == 21 ? "RB Hope" : lnJ == 22 ? "RB Anger" : lnJ == 23 ? "RB Vict" :
                lnJ == 24 ? "RB Desp" : lnJ == 25 ? "RB Conf" : lnJ == 26 ? "RB Myst" : lnJ == 27 ? "RB Immo" : lnJ == 28 ? "Brwn" : lnJ == 30 ? "SH Xtri" : lnJ == 31 ? "SH Kryn" :
                lnJ == 32 ? "SH Cour" : lnJ == 33 ? "SH Brill" : lnJ == 34 ? "SH Just" : lnJ == 35 ? "SH Sound" : lnJ == 36 ? "SH Myst" : lnJ == 37 ? "SH Anger" : lnJ == 38 ? "SH Mystc" : lnJ == 39 ? "SH Frtn" :
                lnJ == 40 ? "SH Immo" : lnJ == 41 ? "SH Illus" : lnJ == 42 ? "Amulet" : lnJ == 43 ? "Ring" : lnJ == 46 ? "Horn" : lnJ == 47 ? "Pod" :
                lnJ == 48 ? "HT Xtri" : lnJ == 50 ? "Scarf" : lnJ == 51 ? "MK Mask" : lnJ == 52 ? "MK Kryn" : "CR Brill");

            if (name != "")
            {
                string printOut = name.PadRight(20);
                for (int lnI = 0; lnI < 7; lnI++)
                {
                    if (romData[byteToUse + 4] % Math.Pow(2, lnI + 1) >= Math.Pow(2, lnI))
                        printOut += (romData[byteToUse] + (romData[byteToUse + 1] * 256)).ToString().PadLeft(10);
                    else
                        printOut += "---".PadLeft(10);
                }

                writer.WriteLine(printOut);
            }

            return writer;
        }

        private void noXPSplitting()
        {
            romData[0x281d2] = romData[0x281d3] = romData[0x281bf] = romData[0x281c0] = romData[0x281c1] = romData[0x281c2] = 0xea;
        }

        private void apprenticeFightAdjustment(Random r1)
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

            // Randomize Patrof apprentice to be 1-10 levels better than you... unless you have free rune on, then make it 1-20 levels better than you.
            romData[0xc020] = (byte)(1 + r1.Next() % (chkWindRune1.Checked || chkWindRune2.Checked || chkWindRune3.Checked || chkWindRune4.Checked || chkWindRune5.Checked ? 20 : 10));
        }

        private void randomizePandam(Random r1)
        {
            if (chkPostBoneRandom.Checked)
            {
                // Randomize who gets to go to the Grime Tower.
                romData[0x6532f] = (byte)((r1.Next() % 7) + 1);
                // Randomize who gets to go to Pandam.  It can be one or two characters.
                romData[0x6566f] = (byte)((r1.Next() % 7) + 1);
                romData[0x65675] = (byte)((r1.Next() % 7) + 1);
            } else if (chkPostBonePandam.Checked)
            {
                textToHex(0x6566e, "", new byte[] { 0xf3, 0xff, 0xe3, 0x57, 0xc6 });
            } else if (chkPostBoneRemote.Checked)
            {
                textToHex(0x6566e, "", new byte[] { 0xf3, 0xff, 0x21, 0x57, 0xc6 });
            }
            if (chkPostBoneGrime.Checked)
            {
                textToHex(0x6532e, "", new byte[] { 0xff, 0x35, 0x53, 0xc6 });
            }
        }

        private void removeUselessTriggers()
        {
            // Cut the mad goose chase for the map
            romData[0x62C83] = 0x10; // Change trigger to the defeat of Romus.

            // Do not require the map to advance the plot
            romData[0x6300d] = 0x10; // Change trigger to the defeat of Romus.

            // Digger Quose
            romData[0x63dd3] = 0x10; // Change trigger to the defeat of Romus.

            // Immediately talk to DR. FAIL by changing trigger to the defeat of Romus.
            romData[0x6e3c0] = romData[0x6e3c6] = romData[0x6e3cc] = romData[0x6e3d2] = romData[0x6e3d8] = romData[0x6e3de] = romData[0x6e3e4] = 0x10;
        }

        private void freeIce()
        {
            // Eygus Sage
            //romData[0x63ba2] = 0x10; // Change trigger to the defeat of Romus.
            //romData[0x63bd8] = 0x10; // Change trigger to the defeat of Romus.

            //// Bone
            //romData[0x655b8] = 0x10;

            //// No runes required in Brush
            //romData[0x6a909] = romData[0x6a90f] = romData[0x6a915] = romData[0x6a91b] = romData[0x6a921] = romData[0x6a927] = romData[0x6a92d] = romData[0x6a92d] = 0x10;

            // Skip Eygus part... hopefully.
            if (chkWindRune1.Checked && !chkWindRune2.Checked)
            {
                textToHex(0x623ae, "Win!", new byte[] { 0xfe, 0x7e, 0x00, 0xf3, 0xf6, 0x22, 0xff, 0xcb, 0x23, 0xc6 });
            } else if (chkWindRune2.Checked)
            {
                textToHex(0x623ae, "Win!", new byte[] { 0xfe, 0x7e, 0x00, 0xf3, 0xf6, 0x22, 0xfe, 0x7d, 0x00, 0xf6, 0x4e, 0xff, 0xcb, 0x23, 0xc6 });
            }

            // "Free Enterprise" the Wind Rune.
            byte[] romPlugin = { 0x22, 0x00, 0xed, 0xc0, 0xea };
            for (int lnI = 0; lnI < romPlugin.Length; lnI++)
                romData[0x49068 + lnI] = romPlugin[lnI];

            int pluginStart = 0xed00;
            byte returnStart = 0xe0;

            if (chkWindRune1.Checked)
            {
                romPlugin = new byte[] {
                    0xa9, 0x01, 0x8f, (byte)(returnStart + 0), 0x50, 0x7e,
                    0xa9, 0x02, 0x8f, (byte)(returnStart + 1), 0x50, 0x7e,
                    0xa9, 0x03, 0x8f, (byte)(returnStart + 2), 0x50, 0x7e,
                    0xa9, 0x04, 0x8f, (byte)(returnStart + 3), 0x50, 0x7e,
                    0xa9, 0x05, 0x8f, (byte)(returnStart + 4), 0x50, 0x7e,
                    0xa9, 0x06, 0x8f, (byte)(returnStart + 5), 0x50, 0x7e,
                    0xa9, 0x07, 0x8f, (byte)(returnStart + 6), 0x50, 0x7e,
                    0xa9, 0x08, 0x8f, (byte)(returnStart + 7), 0x50, 0x7e,
                    0xa9, 0x09, 0x8f, (byte)(returnStart + 8), 0x50, 0x7e,
                    0xa9, 0x0a, 0x8f, (byte)(returnStart + 9), 0x50, 0x7e
                };

                for (int lnI = 0; lnI < romPlugin.Length; lnI++)
                    romData[pluginStart + lnI] = romPlugin[lnI];

                pluginStart += romPlugin.Length;
                returnStart += 10;
            }

            if (chkWindRune2.Checked)
            {
                romPlugin = new byte[] {
                    0xa9, 0x0b, 0x8f, (byte)(returnStart + 0), 0x50, 0x7e,
                    0xa9, 0x0c, 0x8f, (byte)(returnStart + 1), 0x50, 0x7e,
                    0xa9, 0x0d, 0x8f, (byte)(returnStart + 2), 0x50, 0x7e,
                    0xa9, 0x0e, 0x8f, (byte)(returnStart + 3), 0x50, 0x7e,
                    0xa9, 0x0f, 0x8f, (byte)(returnStart + 4), 0x50, 0x7e,
                    0xa9, 0x10, 0x8f, (byte)(returnStart + 5), 0x50, 0x7e,
                    0xa9, 0x12, 0x8f, (byte)(returnStart + 6), 0x50, 0x7e,
                    0xa9, 0x13, 0x8f, (byte)(returnStart + 7), 0x50, 0x7e,
                    0xa9, 0x14, 0x8f, (byte)(returnStart + 8), 0x50, 0x7e,
                    0xa9, 0x15, 0x8f, (byte)(returnStart + 9), 0x50, 0x7e,
                    0xa9, 0x16, 0x8f, (byte)(returnStart + 10), 0x50, 0x7e
                };

                for (int lnI = 0; lnI < romPlugin.Length; lnI++)
                    romData[pluginStart + lnI] = romPlugin[lnI];

                pluginStart += romPlugin.Length;
                returnStart += 11;
            }

            if (chkWindRune3.Checked)
            {
                romPlugin = new byte[] {
                    0xa9, 0x17, 0x8f, (byte)(returnStart + 0), 0x50, 0x7e,
                    0xa9, 0x18, 0x8f, (byte)(returnStart + 1), 0x50, 0x7e,
                    0xa9, 0x1a, 0x8f, (byte)(returnStart + 2), 0x50, 0x7e
                };

                for (int lnI = 0; lnI < romPlugin.Length; lnI++)
                    romData[pluginStart + lnI] = romPlugin[lnI];

                pluginStart += romPlugin.Length;
                returnStart += 3;
            }

            if (chkWindRune4.Checked)
            {
                romPlugin = new byte[] {
                    0xa9, 0x1c, 0x8f, (byte)(returnStart + 0), 0x50, 0x7e,
                    0xa9, 0x1d, 0x8f, (byte)(returnStart + 1), 0x50, 0x7e,
                    0xa9, 0x1e, 0x8f, (byte)(returnStart + 2), 0x50, 0x7e,
                    0xa9, 0x20, 0x8f, (byte)(returnStart + 3), 0x50, 0x7e,
                    0xa9, 0x21, 0x8f, (byte)(returnStart + 4), 0x50, 0x7e
                };

                for (int lnI = 0; lnI < romPlugin.Length; lnI++)
                    romData[pluginStart + lnI] = romPlugin[lnI];

                pluginStart += romPlugin.Length;
                returnStart += 5;
            }

            if (chkWindRune5.Checked)
            {
                romPlugin = new byte[] {
                    0xa9, 0x1f, 0x8f, (byte)(returnStart + 0), 0x50, 0x7e,
                    0xa9, 0x22, 0x8f, (byte)(returnStart + 1), 0x50, 0x7e
                };

                for (int lnI = 0; lnI < romPlugin.Length; lnI++)
                    romData[pluginStart + lnI] = romPlugin[lnI];

                pluginStart += romPlugin.Length;
                returnStart += 2;
            }

            romPlugin = new byte[] {
                0xe2, 0x20, // Start the process that we had to skip with the JSL
                0xad, 0xe7, 0x00,
                0x6b // RTL
            };
            for (int lnI = 0; lnI < romPlugin.Length; lnI++)
                romData[pluginStart + lnI] = romPlugin[lnI];
        }

        private void randomizePison(Random r1)
        {
            // Pison
            string[] randomCharacters = { "The Dragonlord", "A slime", "The Fun Police", "Chaos", "Malroth", "Necrosaro", "Baramos", "Zoma", "Ganon", "Zelda", "Link", "A wizzrobe", "Wario", "Mario", "Luigi", "Bowser", "A goomba",
                "An imp", "Zeromus", "Golbez", "DK Cecil", "Kefka", "Kamil", "Valsu", "Lux", "Wilme", "Lejes", "Olvan", "Esuna" };
            int chosen = r1.Next() % randomCharacters.Length;
            string theChosenOne = randomCharacters[chosen];

            textToHex(0x623fb, "*" + theChosenOne + "* asked me to@take half your money away.@Sorry.@", new byte[] { 0xF3, 0xFE, 0x13, 0x00, 0xF6, 0x28 }); // r1.Next() % randomCharacter.Length

            // Red Pison
            textToHex(0x636e4, "*" + theChosenOne + "* powered me@up so I can kill you!@The money is surely mine!@", new byte[] { 0xF3, 0xFE, 0x15, 0x00, 0xF6, 0x29 });

            // Metal Pison
            string line1 = "";
            string line2 = "";
            string line3 = "Anyway, time to kill@you for good!@";
            switch (chosen)
            {
                case 0:
                    line1 = "*f* defeated *The Dragonlord*.";
                    line2 = "But he did beat *d*.@*d* had more HP,@but was too slow.";
                    break;
                case 1:
                    line1 = "Gooooooday!  I'm the@*Metal Slime!*";
                    line2 = "I may be worth 144,032 XP,@but I will probably@run away!";
                    line3 = "Gooooooo luck!@Slurp!@";
                    break;
                case 2:
                    line1 = "*The Fun Police* wasn't@having fun anymore.";
                    line2 = "Some Dragon Warrior got@too powerful and started@beating them up.";
                    break;
                case 3:
                    line1 = "*Chaos* was defeated@by the four light@warriors.";
                    line2 = "Something about orbs@and fiends... whatever.";
                    break;
                case 4:
                    line1 = "*Malroth* lost.";
                    line2 = "Remember, three heroes@is better than one@or two.";
                    break;
                case 5:
                    line1 = "*Necrosaro* has 8 forms.";
                    line2 = "But now he's out of @control. Something@about *Rosa*.";
                    break;
                case 6:
                    line1 = "*Baramos* was defeated.";
                    line2 = "But apparently there's@a dark world.  I've@been there!";
                    break;
                case 7:
                    line1 = "*Zoma* ran into Erdrick.";
                    line2 = "It didn't go well for him.@He had a Sage's Stone.@GG.";
                    break;
                case 8:
                    line1 = "*Ganon* was killed by *Link*.";
                    line2 = "Even randomized dungeons@were no match for *Link*.";
                    break;
                case 9:
                    line1 = "*Zelda* was kidnapped@by *Ganon*.";
                    line2 = "It's a tradition!";
                    break;
                case 10:
                    line1 = "*Link* was killed by *Ganon*.";
                    line2 = "Yup, that's how@it's SUPPOSED to@happen!";
                    break;
                case 11:
                    line1 = "*The Wizzrobe* casted@too many spells.";
                    line2 = "Wound up getting weaker.@Too bad.";
                    break;
                case 12:
                    line1 = "*Wario* has too much money.";
                    line2 = "Too bad he didn't@have the six@*golden coins*!";
                    break;
                case 13:
                    line1 = "*Mario* was killed@by *Bowser*.";
                    line2 = "All Bowser needed@was hammers!  Can@you believe it?";
                    break;
                case 14:
                    line1 = "*Luigi* is Mario's brother!";
                    line2 = "Such a jealous person...";
                    break;
                case 15:
                    line1 = "*Bowser* went down@a bottomless pit.";
                    line2 = "Something to do with@a bridge, an axe,@and lava.";
                    break;
                case 16:
                    line1 = "*The goomba* was stomped.";
                    line2 = "He really wasn't a@good employer.";
                    break;
                case 17:
                    line1 = "*The imp* only has 6 HP.";
                    line2 = "You can imagine@what happened there.";
                    break;
                case 18:
                    line1 = "*Zeromus* was slain.";
                    line2 = "His Big Bangs dealt@more damage than I will@deal to you!  Weird!";
                    break;
                case 19:
                    line1 = "*Golbez* was brainwashed.";
                    line2 = "Can you believe that?@I think some *Zemus* guy@was involved.";
                    break;
                case 20:
                    line1 = "*Dark Knight Cecil*@became a *Paladin*.@";
                    line2 = "He went up Mt. Ordeals,@beat Milon and himself,@blah blah blah.";
                    break;
                case 21:
                    line1 = "*Kefka* ran into a@level 99 character.";
                    line2 = "Atma Weapon, Offering,@Genji Glove...he was gone@in just one round!";
                    break;
                default:
                    line1 = "I don't know what@happened to *" + theChosenOne + "*.";
                    line2 = "Nope, no idea whatsoever.";
                    break;
            }
            int add1 = textToHex(0x6c13e, line1, new byte[] { 0xF6, 0x6f, 0xfa }, false);
            int add2 = textToHex(0x6c13e + add1, line2, new byte[] { 0xfa }, false);
            int add3 = textToHex(0x6c13e + add1 + add2, line3, new byte[] { 0xF3, 0xF6, 0x2A });
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

            // Menu wraparound (bottom to top - don't work if the menu requires one "screen" only.)
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

            // Music transition speedups (1st character only)
            romData[0x4447e] = romData[0x4447f] = romData[0x44480] = romData[0x44481] = romData[0x444df] = romData[0x444e0] = romData[0x444e1] = romData[0x444e2] = 0xea;
            // Remove delay after levelling up. (1st character only)
            romData[0x444aa] = 0x01;

            // Music transition speedups (2nd character only)
            romData[0x4454a] = romData[0x4454b] = romData[0x4454c] = romData[0x4454d] = romData[0x445ab] = romData[0x445ac] = romData[0x445ad] = romData[0x445ae] = 0xea;
            // Remove delay after levelling up. (2nd character only)
            romData[0x44576] = 0x01;

            // Remove delay of getting key item.
            romData[0x8e24] = 0x01;
            // Remove fight delay for indoor battles
            //romData[0x43072] = 0x01;
            //romData[0x431df] = 0x01;

            // Remove a little fight delay for outdoor battles
            romData[0x4df6e] = 0x00;
            romData[0x4df72] = 0x01;

            // Remove item shop text.
            romPlugin = new byte[] { 0xf6, 0x08 };
            for (int lnI = 0; lnI < romPlugin.Length; lnI++)
                romData[0x60006 + lnI] = romPlugin[lnI];

            romPlugin = new byte[] { 0xf7, 0x36 };
            for (int lnI = 0; lnI < romPlugin.Length; lnI++)
                romData[0x600e7 + lnI] = romPlugin[lnI];

            romPlugin = new byte[] { 0xf7, 0x36 };
            for (int lnI = 0; lnI < romPlugin.Length; lnI++)
                romData[0x60492 + lnI] = romPlugin[lnI];

            textToHex(0x60277, "%");

            romPlugin = new byte[] { 0xf7, 0x36 };
            for (int lnI = 0; lnI < romPlugin.Length; lnI++)
                romData[0x6024d + lnI] = romPlugin[lnI];

            textToHex(0x6029a, "%");

            romPlugin = new byte[] { 0xf7, 0x33 };
            for (int lnI = 0; lnI < romPlugin.Length; lnI++)
                romData[0x604c2 + lnI] = romPlugin[lnI];

            // Remove "King Lemele"'s opening speech.
            romPlugin = new byte[] { 0xf7, 0xfc };
            for (int lnI = 0; lnI < romPlugin.Length; lnI++)
                romData[0x6099e + lnI] = romPlugin[lnI];

            // Weapon store removals
            romPlugin = new byte[] { 0xf6, 0x07 };
            for (int lnI = 0; lnI < romPlugin.Length; lnI++)
                romData[0x60030 + lnI] = romPlugin[lnI];

            romPlugin = new byte[] { 0xf7, 0x36 };
            for (int lnI = 0; lnI < romPlugin.Length; lnI++)
                romData[0x600f9 + lnI] = romPlugin[lnI];

            textToHex(0x602d4, "ATK up *$*%");

            textToHex(0x60141, "Trade-in cost: *$*%");

            romPlugin = new byte[] { 0xf7, 0xfc };
            for (int lnI = 0; lnI < romPlugin.Length; lnI++)
                romData[0x604a7 + lnI] = romPlugin[lnI];

            romPlugin = new byte[] { 0xf7, 0xfc };
            for (int lnI = 0; lnI < romPlugin.Length; lnI++)
                romData[0x604db + lnI] = romPlugin[lnI];

            textToHex(0x60301, "ATK down *$*%");

            textToHex(0x602b7, "%");

            textToHex(0x601ce, "Trade-in rebate: *$* G%");

            textToHex(0x603c1, "No change%");

            // Armor store removals
            romPlugin = new byte[] { 0xf6, 0x05 };
            for (int lnI = 0; lnI < romPlugin.Length; lnI++)
                romData[0x6005d + lnI] = romPlugin[lnI];

            textToHex(0x60110, "Trade-in rebate: *$* G%");
            textToHex(0x60361, "Trade-in cost: *$* G%");

            textToHex(0x60361, "DEF down *$*.%");

            textToHex(0x60334, "DEF up *$*.%");

            textToHex(0x6039b, "No change%");

            textToHex(0x6019d, "Trade-in rebate: *$* G%");

            // Whistle Part 1
            //textToHex(0x6209d, "Have you been to @the castle?@#Go outside to get the@*Whistle*.");
            textToHex(0x6209d, "Go outside to get the@*Whistle*.");
            // Whistle Part 2
            textToHex(0x62286, "Here is the *Whistle*.@", new byte[] { 0xF3, 0xFF, 0x2E, 0x23, 0xC6 });

            // Brantu part 1
            textToHex(0x63012, "I'm *Brantu*.  Let's go@to *Melenam*!", new byte[] { 0xf3, 0xff, 0x78, 0x31, 0xc6 });

            // Post Water-Rune
            textToHex(0x64665, "Now go east for another@*rune*!", new byte[] { 0xfc, 0x1e, 0x00, 0x19, 0x47, 0xc6, 0xfc, 0x1c, 0x00, 0x02, 0x00, 0xc6, 0xfe, 0x1c, 0x00, 0xf6, 0x5b });

            // Remote Control Cave
            textToHex(0x65A9B, "Let's go!@", new byte[] { 0xF3, 0xFC, 0x7D, 0x00, 0xF3, 0x5C, 0xC6, 0xFE, 0x7D, 0x00, 0xF6, 0x4E, 0xF6, 0x4F, 0xF7, 0xF6, 0x4F });

            // Serpent released - remove happy song.
            textToHex(0x68e27, "The power of *Serpent*@is gone.", new byte[] { 0xfc, 0x2d, 0x00, 0x4e, 0x8e, 0xc6, 0xfe, 0x71, 0x00, 0xfe, 0x72, 0x00 });

            // Fortune teller
            textToHex(0x6ac0d, "Go see *Brantu*.");

            // Brantu part 2
            textToHex(0x6ae7c, "Let's see my new plane!", new byte[] { 0xf3, 0xf6, 0x45 });

            // Brantu part 3
            textToHex(0x6af43, "OK, let's go west!", new byte[] { 0xfe, 0x7a, 0x00, 0xfe, 0x31, 0x00, 0xf6, 0x01, 0xf6, 0x46 });

            // Alter curse
            textToHex(0x6b7c7, "Thou art cursed!");

            // Post-Metal Pison
            textToHex(0x6c216, "You won this round!@But I'll still@be around!");

            // Remove curse
            textToHex(0x6bc29, "I was wrong and will@remove the curse.", new byte[] { 0xf3, 0xff, 0xfc, 0xbc, 0xc6 });
            textToHex(0x6bd21, "", new byte[] { 0xfb, 0x0a, 0x00, 0x36, 0xbd, 0xc6 });

            // Shorten post-curse
            textToHex(0x6bd36, "Now go east to get@ the *Wizard Rune*!");

            // Shorten Sage speech
            textToHex(0x6cfa6, "Yes you are from@the future.  Continue your@quest on the airship!");

            // Faster Foma access
            textToHex(0x6db59, "TO PROTECT...", new byte[] { 0xfc, 0x85, 0x00, 0xe6, 0xe7, 0xc6 });

            // Shorten SARO speech
            textToHex(0x6f646, "Good job!  *GORSIA's*@ curse on the *Runes*@ is lifted.", new byte[] { 0xf6, 0x01, 0xf6, 0x03, 0xf6, 0x01, 0xf6, 0x6a });

            // All the old runes at once
            //textToHex(0x0, "", new byte[] { 0xff, 0x52, 0xf4, 0xc6 });

            // Inn
            //romPlugin = new byte[] { 0x28, 0x47, 0x47, 0x5a, 0x0d, 0x82, 0x8c, 0x82, 0x0d, 0x26, 0xfc, 0x00, 0x00, 0x00, 0x00, 0xc6, 0x0d, 0xf9 };
            //for (int lnI = 0; lnI < romPlugin.Length; lnI++)
            //    romData[0x6058b + lnI] = romPlugin[lnI];
        }

        private int textToHex(int startAddress, string text, byte[] extra = null, bool complete = true)
        {
            char[] chars = text.ToCharArray();
            int lnI = 0;
            //for (int lnI = 0; lnI < text.Length; lnI++)
            foreach(char singleChar in chars)
            {
                if (singleChar >= "0".ToCharArray()[0] && singleChar <= "9".ToCharArray()[0])
                    romData[startAddress + lnI] = (byte)(singleChar - 48);
                else if (singleChar >= "A".ToCharArray()[0] && singleChar <= "Z".ToCharArray()[0])
                    romData[startAddress + lnI] = (byte)(singleChar - 33);
                else if (singleChar >= "a".ToCharArray()[0] && singleChar <= "z".ToCharArray()[0])
                    romData[startAddress + lnI] = (byte)(singleChar - 39);
                else if (singleChar == " ".ToCharArray()[0])
                    romData[startAddress + lnI] = 0x0d;
                else if (singleChar == "?".ToCharArray()[0])
                    romData[startAddress + lnI] = 0x56;
                else if (singleChar == ":".ToCharArray()[0])
                    romData[startAddress + lnI] = 0x5a;
                else if (singleChar == ";".ToCharArray()[0])
                    romData[startAddress + lnI] = 0x5b;
                else if (singleChar == "'".ToCharArray()[0])
                    romData[startAddress + lnI] = 0x66;
                else if (singleChar == "\"".ToCharArray()[0])
                    romData[startAddress + lnI] = 0x67;
                else if (singleChar == "-".ToCharArray()[0])
                    romData[startAddress + lnI] = 0x68;
                else if (singleChar == ",".ToCharArray()[0])
                    romData[startAddress + lnI] = 0x69;
                else if (singleChar == ".".ToCharArray()[0])
                    romData[startAddress + lnI] = 0x6a;
                else if (singleChar == "*".ToCharArray()[0])
                    romData[startAddress + lnI] = 0x82; // <b> - Different color start and finish
                else if (singleChar == "!".ToCharArray()[0])
                    romData[startAddress + lnI] = 0x85;
                else if (singleChar == "@".ToCharArray()[0]) // Line break
                    romData[startAddress + lnI] = 0xf9;
                else if (singleChar == "#".ToCharArray()[0])
                    romData[startAddress + lnI] = 0xfa; // Next part of dialog
                else if (singleChar == "$".ToCharArray()[0])
                    romData[startAddress + lnI] = 0x8c; // Gold pieces
                else if (singleChar == "%".ToCharArray()[0])
                { // Shop finish
                    romData[startAddress + lnI] = 0xfc; lnI++;
                    romData[startAddress + lnI] = 0x00; lnI++;
                    romData[startAddress + lnI] = 0x00; lnI++;
                    romData[startAddress + lnI] = 0x00; lnI++;
                    romData[startAddress + lnI] = 0x00; lnI++;
                    romData[startAddress + lnI] = 0xc6;
                }
                lnI++;
            }
            if (extra != null)
            {
                foreach (byte single in extra)
                {
                    romData[startAddress + lnI] = single;
                    lnI++;
                }
            }
            if (complete)
            {
                romData[startAddress + lnI] = 0xf7; // End of dialog
                lnI++;
            }
            return lnI;
        }

        private void doubleWalk()
        {
            // Double walking speed
            for (int lnI = 0x423f7; lnI < 0x42526; lnI++)
            {
                if (romData[lnI] == 0xfe) romData[lnI] = 0xfc;
                if (romData[lnI] == 0x02) romData[lnI] = 0x04;
            }
            // This stops the character each square.
            romData[0x42095] = 0x08;

            // This prevents screen tearing
            byte[] romPlugin = new byte[] { 0x22, 0xf0, 0xf9, 0xc0 };
            for (int lnI = 0; lnI < romPlugin.Length; lnI++)
                romData[0xfae5 + lnI] = romPlugin[lnI];

            romPlugin = new byte[] { 0x22, 0x61, 0x8c, 0xc2, 0x22, 0x61, 0x8c, 0xc2, 0x6b };
            for (int lnI = 0; lnI < romPlugin.Length; lnI++)
                romData[0xf9f0 + lnI] = romPlugin[lnI];

            // This prevent psycho allies and NPCs moving around like a Press Your Luck board.
            romData[0x40418] = romData[0x404de] = romData[0x405b1] = romData[0x40859] = romData[0x41187] = 0x08;
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
                    // If past is immediately unlocked, immediately go to 63 after zone 11.  Immediately have a big minimum monster after zone 25.
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
                        romData[byteToUse + lnJ + 11] = monsterLegalSpells[r1.Next() % monsterLegalSpells.Length];
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

        private void randomizeMonsterDrops(Random r1)
        {
            byte[] commonItems = { 0x01, 0x02, 0x0b, 0x0c, 0x0d, 0x11, 0x12, 0x13, 0x14, 0x29, 0x2d, 0x2e, 0x30, 0x32, 0x34, 0x35, 0x38, 0x39, 0x3a, 0x43, 0x44 };
            byte[] rareItems = { 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b };
            byte[] jewelItems = { 0x47, 0x48, 0x49, 0x4a, 0x4b, 0x4c, 0x4d };
            byte[] equipItems = { 0x65, 0x66, 0x67, 0x68, 0x69, 0x6a, 0x6b, 0x6c,
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
                                0xc0, 0xc1, 0xc2, 0xc7, 0xc8, 0xc9, 0xca, 0xcb };
            // First, let's randomize the actual drops
            // The first one is fixed; no drop.  The second four are for the tricks.  The rest are random.
            for (int lnI = 0; lnI < 30; lnI++)
            {
                int byteToUse = 0x8a18 + (lnI * 16);
                for (int lnJ = 0; lnJ < 16; lnJ++)
                {
                    romData[byteToUse + lnJ] = 0x00;
                    if (lnI == 1)
                    {
                        romData[byteToUse + lnJ] = (commonItems[r1.Next() % commonItems.Length]);
                    }
                    else if (lnI == 2)
                    {
                        romData[byteToUse + lnJ] = (rareItems[r1.Next() % rareItems.Length]);
                    }
                    else if (lnI == 3)
                    {
                        romData[byteToUse + lnJ] = (jewelItems[r1.Next() % jewelItems.Length]);
                    }
                    else if (lnI == 4)
                    {
                        romData[byteToUse + lnJ] = (equipItems[r1.Next() % equipItems.Length]);
                    }
                    else if (lnI > 0) // If lnI == 0, keep everything at 0.
                    {
                        if (lnJ == 0 || r1.Next() % 2 == 0)
                        {
                            int itemToUse = r1.Next() % 16;
                            if (itemToUse < 8)
                                romData[byteToUse + lnJ] = (commonItems[r1.Next() % commonItems.Length]);
                            else if (itemToUse < 12)
                                romData[byteToUse + lnJ] = (jewelItems[r1.Next() % jewelItems.Length]);
                            else if (itemToUse < 14)
                                romData[byteToUse + lnJ] = (rareItems[r1.Next() % rareItems.Length]);
                            else
                                romData[byteToUse + lnJ] = (equipItems[r1.Next() % equipItems.Length]);
                        }
                        else
                            break;
                    }
                }
            }

            for (int lnI = 0; lnI < monsterRanking.Length; lnI++)
            {
                int byteToUse = 0x72f4 + (monsterRanking[lnI] * 42);
                if (romData[byteToUse] == 0x46 || romData[byteToUse] == 0x00) continue; // Do not randomize Gorsia or blank monsters.
                if (monsterRanking[lnI] == 0x03)
                    romData[byteToUse + 36] = 0x01;
                else if (monsterRanking[lnI] == 0x43)
                    romData[byteToUse + 36] = 0x02;
                else if (monsterRanking[lnI] == 0x44)
                    romData[byteToUse + 36] = 0x03;
                else if (monsterRanking[lnI] == 0x45)
                    romData[byteToUse + 36] = 0x04;
                else if (r1.Next() % 3 == 0)
                {
                    romData[byteToUse + 36] = (byte)(0x05 + (r1.Next() % 25));
                } else
                {
                    romData[byteToUse + 36] = 0x00;
                }
            }
        }

        private void randomizeHeroSpells(Random r1)
        {
            for (int lnI = 0; lnI < 7; lnI++)
            {
                int byteToUse = 0x62bd + (32 * lnI);

                List<byte> actualSpells = new List<byte>();
                if (!chkLearnNoMagic.Checked)
                {
                    int maxSpells = (chkHeroSameSpell.Checked ? 16 : lnI == 0 ? 12 : lnI == 1 ? 10 : lnI == 2 ? 16 : lnI == 3 ? 7 : lnI == 4 ? 5 : lnI == 5 ? 16 : 16);
                    List<byte> actualLegalSpells = new List<byte>();
                    if (chkSpellLearning.Checked)
                    {
                        switch (lnI)
                        {
                            case 0:
                                actualLegalSpells = new List<byte>() { 0x01, 0x18, 0x2D, 0x0C, 0x19, 0x03, 0x15, 0x02, 0x28, 0x2E, 0x1A, 0x0D };
                                break;
                            case 1:
                                actualLegalSpells = new List<byte>() { 0x18, 0x01, 0x15, 0x16, 0x02, 0x19, 0x2D, 0x1B, 0x1A, 0x2E };
                                break;
                            case 2:
                                actualLegalSpells = new List<byte>() { 0x03, 0x22, 0x1B, 0x18, 0x2D, 0x04, 0x0E, 0x15, 0x21, 0x28, 0x16, 0x0F, 0x1E, 0x2E, 0x1A, 0x29 };
                                break;
                            case 3:
                                actualLegalSpells = new List<byte>() { 0x01, 0x22, 0x0C, 0x02, 0x1B, 0x0D, 0x28 };
                                break;
                            case 4:
                                actualLegalSpells = new List<byte>() { 0x05, 0x06, 0x10, 0x07, 0x11 };
                                break;
                            case 5:
                                actualLegalSpells = new List<byte>() { 0x18, 0x03, 0x16, 0x2D, 0x15, 0x19, 0x2E, 0x21, 0x1C, 0x1D, 0x1B, 0x1A, 0x1F, 0x1E, 0x2F, 0x23 };
                                break;
                            case 6:
                                actualLegalSpells = new List<byte>() { 0x01, 0x22, 0x03, 0x15, 0x0C, 0x02, 0x17, 0x0E, 0x04, 0x28, 0x1B, 0x1D, 0x0D, 0x0F, 0x1E, 0x29 };
                                break;
                        }
                    }
                    if (chkSpellLearning.Checked)
                    {
                        byte[] byteSpells = shuffle(actualLegalSpells.ToArray(), r1);
                        actualSpells = byteSpells.ToList();
                    }
                    else
                    {
                        for (int lnJ = 0; lnJ < maxSpells; lnJ++)
                        {
                            actualSpells.Add(legalSpells[r1.Next() % legalSpells.Length]);
                            for (int lnK = 0; lnK < lnJ; lnK++)
                                if (actualSpells[lnJ] == actualSpells[lnK]) { actualSpells.RemoveAt(actualSpells.Count - 1); lnJ--; break; }
                        }
                    }
                }

                int[] spellLevels = inverted_power_curve(1, 45, actualSpells.Count, 1, r1);

                for (int lnJ = 0; lnJ < 32; lnJ++)
                    romData[byteToUse + lnJ] = 0;
                for (int lnJ = 0; lnJ < actualSpells.Count; lnJ++)
                {
                    romData[byteToUse + lnJ] = actualSpells[lnJ];
                    romData[byteToUse + lnJ + 16] = (byte)spellLevels[lnJ];
                }
            }

            // OLD METHOD:
            // Learn spells as long as you don't duplicate another spell.
            // Lux/Wilme get one duplicate chance only.  Kamil and Olvan get 10 chances, Lejes gets 50 chances, and Esuna and Valsu get 100 chances. (Lejes gets to equip more stuff than Esuna and Valsu)
            // If same hero stats is checked, then all heroes get 20 chances.
            //int duplicateChances = (chkHeroSameSpell.Checked ? 20 : lnI == 3 || lnI == 4 ? 1 : lnI == 0 || lnI == 1 ? 10 : lnI == 6 ? 50 : 100);
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

        private void noStoreItems()
        {
            for (int lnI = 0; lnI < 40; lnI++)
            {
                int byteToUse = 0x8308 + (lnI * 27);
                
                for (int lnJ = 0; lnJ < 22; lnJ++)
                    romData[byteToUse + lnJ] = 0;
            }
        }

        private byte[] shuffle(byte[] items, Random r1)
        {
            for (int lnI = 0; lnI <= items.Length * 7; lnI++)
            {
                int item1 = r1.Next() % items.Length;
                int item2 = r1.Next() % items.Length;
                byte temp = items[item1];
                items[item1] = items[item2];
                items[item2] = temp;
            }
            return items;
        }

        private void randomizeStores(Random r1)
        {
            // 46 weapons
            byte[] weapons = {
                0x65, 0x66, 0x67, 0x68, 0x69, 0x6a, 0x6b, 0x6c,
                0x6d, 0x6e, 0x6f, 0x70, 0x71, 0x72, 0x73, 0x74,
                0x77, 0x78, 0x79, 0x7a, 0x7b, 0x7c, 0x7d, 0x7e,
                0x7f, 0x80, 0x81, 0x82, 0x83, 0x85, 0x86, 0x87,
                0x88, 0x89, 0x8a, 0x8b, 0x8d, 0x8e, 0x8f, 0x90,
                0x91, 0x92, 0x93, 0x94, 0x95, 0x96
            };
            // 48 armor
            byte[] armor = {
                0x97, 0x98, 0x99, 0x9a, 0x9b, 0x9c, 0x9d, 0x9e,
                0x9f, 0xa0, 0xa1, 0xa2, 0xa3, 0xa4, 0xa5, 0xa6,
                0xa7, 0xa8, 0xa9, 0xaa, 0xab, 0xac, 0xad, 0xae,
                0xaf, 0xb0, 0xb1, 0xb2, 0xb3, 0xb5, 0xb6, 0xb7,
                0xb8, 0xb9, 0xba, 0xbb, 0xbc, 0xbd, 0xbe, 0xbf,
                0xc0, 0xc1, 0xc2, 0xc7, 0xc8, 0xc9, 0xca, 0xcb
            };

            byte[] commonItems = { 0x0b, 0x0c, 0x0d };
            byte[] magicItems = { 0x01, 0x02, 0x11, 0x12, 0x13, 0x14, 0x29, 0x2d, 0x2e, 0x30, 0x32, 0x34, 0x35, 0x38, 0x39, 0x3a, 0x43, 0x44 };
            byte[] jewelItems = { 0x47, 0x48, 0x49, 0x4a, 0x4b, 0x4c, 0x4d };
            byte[] rareItems = { 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b };

            byte[] items = {
                0x01, 0x02, 0x0b, 0x0c, 0x0d, 0x11, 0x12, 0x13,
                0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b,
                0x29, 0x2d, 0x2e, 0x30, 0x32, 0x34, 0x35, 0x38,
                0x39, 0x3a, 0x43, 0x44, 0x47, 0x48, 0x49, 0x4a,
                0x4b, 0x4c, 0x4d
            };
            if (chkNoSeeds.Checked)
            {
                items = new byte[] {
                    0x01, 0x02, 0x0b, 0x0c, 0x0d, 0x11, 0x12, 0x13,
                    0x14, 
                    0x29, 0x2d, 0x2e, 0x30, 0x32, 0x34, 0x35, 0x38,
                    0x39, 0x3a, 0x43, 0x44, 0x47, 0x48, 0x49, 0x4a,
                    0x4b, 0x4c, 0x4d
                };
            }

            int weaponMarker = 0;
            int armorMarker = 0;
            int magicMarker = 0;
            int jewelMarker = 0;
            int rareMarker = 0;

            // 26 stores in total.
            // Shuffle weapons, armor, magic items, jewel items, and rare items like a deck of cards.
            weapons = shuffle(weapons, r1);
            armor = shuffle(armor, r1);
            magicItems = shuffle(magicItems, r1);
            jewelItems = shuffle(jewelItems, r1);
            rareItems = shuffle(rareItems, r1);

            for (int lnI = 0; lnI < 40; lnI++)
            {
                int byteToUse = 0x8308 + (lnI * 27);

                if (lnI == 0x04 || lnI == 0x05 || lnI == 0x07 || lnI == 0x0d ||
                    lnI == 0x10 || lnI == 0x18 || lnI == 0x1a || lnI == 0x1e ||
                    lnI == 0x22 || lnI == 0x23 || lnI == 0x24 || lnI == 0x25 || lnI == 0x26 || lnI == 0x27)
                    continue;

                List<byte> cityWeapons = new List<byte>();
                // Weapons at bytes 0-4, armor at bytes 5-12, items at bytes 13-21.  I reserve the right to place weapons in armor stores and vice versa.
                for (int lnJ = 0; lnJ < 2 + (r1.Next() % 3); lnJ++)
                {
                    //bool duplicate = true;
                    //byte currentWeapon = 0;

                    //while (duplicate)
                    //{
                    //    currentWeapon = weapons[r1.Next() % weapons.Length];
                    //    duplicate = false;
                    //    for (int lnK = 0; lnK < cityWeapons.Count; lnK++)
                    //        if (currentWeapon == cityWeapons[lnK]) { duplicate = true; break; }
                    //}
                    //cityWeapons.Sort();
                    cityWeapons.Add(weapons[weaponMarker]);
                    weaponMarker++;
                    if (weaponMarker == weapons.Length)
                    {
                        weapons = shuffle(weapons, r1);
                        weaponMarker = 0;
                    }
                }
                cityWeapons.Sort();
                for (int lnJ = 0; lnJ < 5; lnJ++)
                    romData[byteToUse + lnJ] = (byte)(lnJ < cityWeapons.Count ? cityWeapons[lnJ] : 0);

                List<byte> cityArmor = new List<byte>();
                for (int lnJ = 0; lnJ < 2 + (r1.Next() % 4); lnJ++)
                {
                    //bool duplicate = true;
                    //byte currentWeapon = 0;
                    //while (duplicate)
                    //{
                    //    currentWeapon = armor[r1.Next() % armor.Length];
                    //    duplicate = false;
                    //    for (int lnK = 0; lnK < cityArmor.Count; lnK++)
                    //        if (currentWeapon == cityArmor[lnK]) { duplicate = true; break; }
                    //}
                    //cityArmor.Sort();
                    //cityArmor.Add(currentWeapon);
                    cityArmor.Add(armor[armorMarker]);
                    armorMarker++;
                    if (armorMarker == armor.Length)
                    {
                        armor = shuffle(armor, r1);
                        armorMarker = 0;
                    }
                }
                for (int lnJ = 0; lnJ < 8; lnJ++)
                    romData[byteToUse + 5 + lnJ] = (byte)(lnJ < cityArmor.Count ? cityArmor[lnJ] : 0);

                romData[byteToUse + 13] = commonItems[r1.Next() % commonItems.Length];

                List<byte> cityItems = new List<byte>();
                for (int lnJ = 0; lnJ < 4; lnJ++)
                {
                    //bool duplicate = true;
                    //byte currentItem = 0;
                    //while (duplicate)
                    //{
                    //    currentItem = items[r1.Next() % items.Length];
                    //    duplicate = false;
                    //    for (int lnK = 0; lnK < cityItems.Count; lnK++)
                    //        if (currentItem == cityItems[lnK]) { duplicate = true; break; }
                    //}
                    //cityItems.Sort();
                    //cityItems.Add(currentItem);
                    cityItems.Add(magicItems[magicMarker]);
                    magicMarker++;
                    if (magicMarker == magicItems.Length)
                    {
                        magicItems = shuffle(magicItems, r1);
                        magicMarker = 0;
                    }
                }
                for (int lnJ = 0; lnJ < 4; lnJ++)
                    romData[byteToUse + 14 + lnJ] = (byte)(lnJ < cityItems.Count ? cityItems[lnJ] : 0);

                cityItems = new List<byte>();
                for (int lnJ = 18; lnJ < 20; lnJ++)
                {
                    //bool duplicate = true;
                    //byte currentItem = 0;
                    //while (duplicate)
                    //{
                    //    currentItem = items[r1.Next() % items.Length];
                    //    duplicate = false;
                    //    for (int lnK = 0; lnK < cityItems.Count; lnK++)
                    //        if (currentItem == cityItems[lnK]) { duplicate = true; break; }
                    //}
                    //cityItems.Sort();
                    //cityItems.Add(currentItem);
                    cityItems.Add(jewelItems[jewelMarker]);
                    jewelMarker++;
                    if (jewelMarker == jewelItems.Length)
                    {
                        jewelItems = shuffle(jewelItems, r1);
                        jewelMarker = 0;
                    }
                }
                for (int lnJ = 0; lnJ < 2; lnJ++)
                    romData[byteToUse + 18 + lnJ] = (byte)(lnJ < cityItems.Count ? cityItems[lnJ] : 0);

                if (!chkNoSeeds.Checked)
                {
                    cityItems = new List<byte>();
                    for (int lnJ = 20; lnJ < 21; lnJ++)
                    {
                        //bool duplicate = true;
                        //byte currentItem = 0;
                        //while (duplicate)
                        //{
                        //    currentItem = items[r1.Next() % items.Length];
                        //    duplicate = false;
                        //    for (int lnK = 0; lnK < cityItems.Count; lnK++)
                        //        if (currentItem == cityItems[lnK]) { duplicate = true; break; }
                        //}
                        //cityItems.Sort();
                        //cityItems.Add(currentItem);
                        cityItems.Add(rareItems[rareMarker]);
                        rareMarker++;
                        if (rareMarker == rareItems.Length)
                        {
                            rareItems = shuffle(rareItems, r1);
                            rareMarker = 0;
                        }
                    }
                    for (int lnJ = 0; lnJ < 1; lnJ++)
                        romData[byteToUse + 20 + lnJ] = (byte)(lnJ < cityItems.Count ? cityItems[lnJ] : 0);
                } else
                {
                    romData[byteToUse + 20] = 0x00;
                }
                romData[byteToUse + 21] = 0x00;
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

            if (chkHeroSameEquip.Checked)
            {
                for (int lnI = 0; lnI < weapons.Length; lnI++)
                {
                    int byteToUse = 0x639d + (10 * weapons[lnI]);
                    byte whoEquip = 0;
                    if (r1.Next() % 100 < 40) whoEquip += 0x01;
                    if (r1.Next() % 100 < 40) whoEquip += 0x02;
                    if (r1.Next() % 100 < 40) whoEquip += 0x04;
                    if (r1.Next() % 100 < 40) whoEquip += 0x08;
                    if (r1.Next() % 100 < 40) whoEquip += 0x10;
                    if (r1.Next() % 100 < 40) whoEquip += 0x20;
                    if (r1.Next() % 100 < 40) whoEquip += 0x40;
                    romData[byteToUse + 4] = whoEquip;
                }
                for (int lnI = 0; lnI < armor.Length; lnI++)
                {
                    int byteToUse = 0x659b + (17 * armor[lnI]);
                    byte whoEquip = 0;
                    if (r1.Next() % 100 < 40) whoEquip += 0x01;
                    if (r1.Next() % 100 < 40) whoEquip += 0x02;
                    if (r1.Next() % 100 < 40) whoEquip += 0x04;
                    if (r1.Next() % 100 < 40) whoEquip += 0x08;
                    if (r1.Next() % 100 < 40) whoEquip += 0x10;
                    if (r1.Next() % 100 < 40) whoEquip += 0x20;
                    if (r1.Next() % 100 < 40) whoEquip += 0x40;
                    romData[byteToUse + 4] = whoEquip;
                }
                for (int lnI = 0; lnI < accessory.Length; lnI++)
                {
                    int byteToUse = 0x659b + (17 * accessory[lnI]);
                    byte whoEquip = 0;
                    if (r1.Next() % 100 < 40) whoEquip += 0x01;
                    if (r1.Next() % 100 < 40) whoEquip += 0x02;
                    if (r1.Next() % 100 < 40) whoEquip += 0x04;
                    if (r1.Next() % 100 < 40) whoEquip += 0x08;
                    if (r1.Next() % 100 < 40) whoEquip += 0x10;
                    if (r1.Next() % 100 < 40) whoEquip += 0x20;
                    if (r1.Next() % 100 < 40) whoEquip += 0x40;
                    romData[byteToUse + 4] = whoEquip;
                }
            } else
            {
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

            // Also adjust apprentice fight XP rewards
            int xpChar = romData[0x280c2] + (256 * romData[0x280c3]);
            xpChar *= (trkExperience.Value * 20 / 100);
            romData[0x280c2] = (byte)(xpChar % 256);
            romData[0x280c3] = (byte)(xpChar / 256);
        }

        private void goldRequirements(Random r1)
        {
            // Weapons
            for (int lnI = 0; lnI < 51; lnI++)
            {
                int byteToUse = 0x639d + (10 * lnI);
                statAdjust(r1, byteToUse + 2, 2, trkGoldReq.Value / 10, 1.0, chkGoldMin.Checked);
            }
            // Armor/Accessories
            for (int lnI = 0; lnI < 51; lnI++)
            {
                int byteToUse = 0x659b + (17 * lnI);
                statAdjust(r1, byteToUse + 2, 2, trkGoldReq.Value / 10, 1.0, chkGoldMin.Checked);
            }
            // Items
            for (int lnI = 0; lnI < 100; lnI++)
            {
                int byteToUse = 0x6c94 + (9 * lnI);
                statAdjust(r1, byteToUse + 2, 2, trkGoldReq.Value / 10, 1.0, chkGoldMin.Checked);
            }
            // Inns
            for (int lnI = 0; lnI < 38; lnI++)
            {
                int byteToUse = 0x8308 + (27 * lnI);
                if (romData[byteToUse + 22] == 0) continue;
                statAdjust(r1, byteToUse + 22, 2, trkGoldReq.Value / 10, 1.0, chkGoldMin.Checked);
            }
        }

        private void monsterStats(Random r1)
        {
            for (int lnI = 0; lnI < 90; lnI++)
            {
                int byteToUse = 0x72f4 + (42 * lnI);
                if (romData[byteToUse] == 0x46 || romData[byteToUse] == 0x00) continue; // Do not randomize Gorsia or blank monsters.

                statAdjust(r1, byteToUse + 1, 2, trkMonsterStats.Value / 10, 1.0, chkMonsterStatMin.Checked);
                statAdjust(r1, byteToUse + 3, 2, trkMonsterStats.Value / 10, 1.0, chkMonsterStatMin.Checked);
                statAdjust(r1, byteToUse + 5, 2, trkMonsterStats.Value / 10, 0.5, chkMonsterStatMin.Checked);
                statAdjust(r1, byteToUse + 7, 2, trkMonsterStats.Value / 10, 0.5, chkMonsterStatMin.Checked);
                statAdjust(r1, byteToUse + 9, 1, trkMonsterStats.Value / 10, 0.25, chkMonsterStatMin.Checked);
                statAdjust(r1, byteToUse + 10, 1, trkMonsterStats.Value / 10, 0.25, chkMonsterStatMin.Checked);
                statAdjust(r1, byteToUse + 27, 1, trkMonsterStats.Value / 10, 0.5, chkMonsterStatMin.Checked, 99);
                statAdjust(r1, byteToUse + 28, 1, trkMonsterStats.Value / 10, 0.5, chkMonsterStatMin.Checked, 99);
                statAdjust(r1, byteToUse + 29, 1, trkMonsterStats.Value / 10, 0.5, chkMonsterStatMin.Checked, 99);
                statAdjust(r1, byteToUse + 30, 1, trkMonsterStats.Value / 10, 0.5, chkMonsterStatMin.Checked, 99);
                statAdjust(r1, byteToUse + 31, 1, trkMonsterStats.Value / 10, 0.5, chkMonsterStatMin.Checked, 99);
                statAdjust(r1, byteToUse + 32, 1, trkMonsterStats.Value / 10, 0.5, chkMonsterStatMin.Checked, 99);
                statAdjust(r1, byteToUse + 33, 1, trkMonsterStats.Value / 10, 0.5, chkMonsterStatMin.Checked, 99);
                if (!chkNoXPGPRando.Checked)
                    statAdjust(r1, byteToUse + 34, 2, trkMonsterStats.Value / 10, 1.0, chkMonsterStatMin.Checked);
                if (chk9999Defense.Checked)
                {
                    romData[byteToUse + 8] = 0x27;
                    romData[byteToUse + 7] = 0x0f;
                }
                if (romData[byteToUse + 8] >= 0x03)
                    romData[byteToUse + 9] = (byte)(romData[byteToUse + 9] > 150 ? 150 : romData[byteToUse + 9]);
                //romData[byteToUse + 34] = 0xff;
                //romData[byteToUse + 35] = 0xff;
            }

            // Also adjust apprentice fight XP rewards
            statAdjust(r1, 0x280c2, 2, trkMonsterStats.Value / 10, 1.0, chkMonsterStatMin.Checked, 800);

        }

        private void equipmentStats(Random r1)
        {
            // Weapons
            for (int lnI = 0; lnI < 51; lnI++)
            {
                int byteToUse = 0x639d + (10 * lnI);
                statAdjust(r1, byteToUse, 2, trkEquipPowers.Value / 10, 1.0, chkEquipMin.Checked);
            }
            // Armor/Accessories
            for (int lnI = 0; lnI < 53; lnI++)
            {
                int byteToUse = 0x659b + (17 * lnI);
                statAdjust(r1, byteToUse, 2, trkEquipPowers.Value / 10, 1.0, chkEquipMin.Checked);
            }
        }

        private void spellCosts(Random r1)
        {
            for (int lnI = 0; lnI < 61; lnI++)
            {
                int byteToUse = 0x7018 + (12 * lnI);
                statAdjust(r1, byteToUse + 3, 1, trkSpellCosts.Value / 10, 0.5, chkSpellMin.Checked);
                statAdjust(r1, byteToUse, 2, trkSpellCosts.Value / 10, 1.0, chkSpellMin.Checked);
            }
        }

        private void heroStats(Random r1)
        {
            double hp = ScaleValueDouble(1, trkHeroStats.Value / 10, 1.0, r1, chkHeroStatMin.Checked);
            double mp = ScaleValueDouble(1, trkHeroStats.Value / 10, 1.0, r1, chkHeroStatMin.Checked);
            double power = ScaleValueDouble(1, trkHeroStats.Value / 10, 1.0, r1, chkHeroStatMin.Checked);
            double guard = ScaleValueDouble(1, trkHeroStats.Value / 10, 1.0, r1, chkHeroStatMin.Checked);
            double magic = ScaleValueDouble(1, trkHeroStats.Value / 10, 1.0, r1, chkHeroStatMin.Checked);
            double speed = ScaleValueDouble(1, trkHeroStats.Value / 10, 1.0, r1, chkHeroStatMin.Checked);
            double xp = ScaleValueDouble(1, trkHeroStats.Value / 10, 0.5, r1, chkHeroStatMin.Checked);

            double hp2 = ScaleValueDouble(1, trkHeroGrowth.Value / 10, 0.5, r1, chkHeroGrowthMin.Checked);
            double mp2 = ScaleValueDouble(1, trkHeroGrowth.Value / 10, 0.5, r1, chkHeroGrowthMin.Checked);
            double power2 = ScaleValueDouble(1, trkHeroGrowth.Value / 10, 0.5, r1, chkHeroGrowthMin.Checked);
            double guard2 = ScaleValueDouble(1, trkHeroGrowth.Value / 10, 0.5, r1, chkHeroGrowthMin.Checked);
            double magic2 = ScaleValueDouble(1, trkHeroGrowth.Value / 10, 0.5, r1, chkHeroGrowthMin.Checked);
            double speed2 = ScaleValueDouble(1, trkHeroGrowth.Value / 10, 0.5, r1, chkHeroGrowthMin.Checked);

            for (int lnI = 0; lnI < 7; lnI++)
            {
                int byteToUse = 0x623f + (18 * lnI);
                if (chkSameRando.Checked)
                {
                    if (chkHeroSameStats.Checked)
                    {
                        romData[byteToUse] = (byte)(Math.Max(0, 16.7 * hp)); // Starting HP
                        romData[byteToUse + 2] = (byte)(Math.Max(0, 6.9 * mp)); // Starting MP
                        romData[byteToUse + 4] = (byte)(Math.Max(0, 4.1 * power)); // Starting Power
                        romData[byteToUse + 5] = (byte)(Math.Max(0, 4.6 * guard)); // Starting Guard
                        romData[byteToUse + 6] = (byte)(Math.Max(0, 3.1 * magic)); // Starting Magic
                        romData[byteToUse + 7] = (byte)(Math.Max(0, 3.6 * speed)); // Starting Speed
                        romData[byteToUse + 8] = (byte)(Math.Max(1, 6.0 * hp2)); // HP Boost
                        romData[byteToUse + 9] = (byte)(Math.Max(1, 3.1 * mp2)); // MP Boost
                        romData[byteToUse + 10] = (byte)(Math.Max(1, 3.0 * power2)); // Power Boost
                        romData[byteToUse + 11] = (byte)(Math.Max(1, 3.4 * guard2)); // Guard Boost
                        romData[byteToUse + 12] = (byte)(Math.Max(1, 3.1 * magic2)); // Magic Boost
                        romData[byteToUse + 13] = (byte)(Math.Max(1, 3.6 * speed2)); // Speed Boost
                        romData[byteToUse + 17] = (byte)(Math.Max(0, 21.3 * xp)); // Starting Experience
                    }
                    else
                    {
                        romData[byteToUse] = (byte)(Math.Max(1, romData[byteToUse + 0] * hp)); // Starting HP
                        romData[byteToUse + 2] = (byte)(Math.Max(0, romData[byteToUse + 2] * mp)); // Starting MP
                        romData[byteToUse + 4] = (byte)(Math.Max(0, romData[byteToUse + 4] * power)); // Starting Power
                        romData[byteToUse + 5] = (byte)(Math.Max(0, romData[byteToUse + 5] * guard)); // Starting Guard
                        romData[byteToUse + 6] = (byte)(Math.Max(0, romData[byteToUse + 6] * magic)); // Starting Magic
                        romData[byteToUse + 7] = (byte)(Math.Max(0, romData[byteToUse + 7] * speed)); // Starting Speed
                        romData[byteToUse + 8] = (byte)(Math.Max(1, romData[byteToUse + 8] * hp2)); // HP Boost
                        romData[byteToUse + 9] = (byte)(Math.Max(1, romData[byteToUse + 9] * mp2)); // MP Boost
                        romData[byteToUse + 10] = (byte)(Math.Max(1, romData[byteToUse + 10] * power2)); // Power Boost
                        romData[byteToUse + 11] = (byte)(Math.Max(1, romData[byteToUse + 11] * guard2)); // Guard Boost
                        romData[byteToUse + 12] = (byte)(Math.Max(1, romData[byteToUse + 12] * magic2)); // Magic Boost
                        romData[byteToUse + 13] = (byte)(Math.Max(1, romData[byteToUse + 13] * speed2)); // Speed Boost
                        romData[byteToUse + 17] = (byte)(Math.Max(0, romData[byteToUse + 17] * xp)); // Starting Experience
                    }
                }
                else
                {
                    if (chkHeroSameStats.Checked)
                    {
                        romData[byteToUse] = (byte)(ScaleValue(16.7, trkHeroStats.Value / 10, 1.0, r1, chkHeroStatMin.Checked)); // Starting HP
                        romData[byteToUse + 2] = (byte)(ScaleValue(6.9, trkHeroStats.Value / 10, 1.0, r1, chkHeroStatMin.Checked)); // Starting MP
                        romData[byteToUse + 4] = (byte)(ScaleValue(4.1, trkHeroStats.Value / 10, 1.0, r1, chkHeroStatMin.Checked)); // Starting Power
                        romData[byteToUse + 5] = (byte)(ScaleValue(4.6, trkHeroStats.Value / 10, 1.0, r1, chkHeroStatMin.Checked)); // Starting Guard
                        romData[byteToUse + 6] = (byte)(ScaleValue(3.1, trkHeroStats.Value / 10, 1.0, r1, chkHeroStatMin.Checked)); // Starting Magic
                        romData[byteToUse + 7] = (byte)(ScaleValue(3.6, trkHeroStats.Value / 10, 1.0, r1, chkHeroStatMin.Checked)); // Starting Speed
                        romData[byteToUse + 8] = (byte)(ScaleValue(6, trkHeroGrowth.Value / 10, 0.5, r1, chkHeroGrowthMin.Checked)); // HP Boost
                        romData[byteToUse + 9] = (byte)(ScaleValue(3.1, trkHeroGrowth.Value / 10, 0.5, r1, chkHeroGrowthMin.Checked)); // MP Boost
                        romData[byteToUse + 10] = (byte)(ScaleValue(3, trkHeroGrowth.Value / 10, 0.5, r1, chkHeroGrowthMin.Checked)); // Power Boost
                        romData[byteToUse + 11] = (byte)(ScaleValue(3.4, trkHeroGrowth.Value / 10, 0.5, r1, chkHeroGrowthMin.Checked)); // Guard Boost
                        romData[byteToUse + 12] = (byte)(ScaleValue(3.1, trkHeroGrowth.Value / 10, 0.5, r1, chkHeroGrowthMin.Checked)); // Magic Boost
                        romData[byteToUse + 13] = (byte)(ScaleValue(3.6, trkHeroGrowth.Value / 10, 0.5, r1, chkHeroGrowthMin.Checked)); // Speed Boost
                        romData[byteToUse + 17] = (byte)(ScaleValue(21.3, trkHeroGrowth.Value / 10, 0.5, r1, chkHeroGrowthMin.Checked)); // Starting Experience
                    }
                    else
                    {
                        statAdjust(r1, byteToUse, 2, trkHeroStats.Value / 10, 1.0, chkHeroStatMin.Checked); // Starting HP
                        statAdjust(r1, byteToUse + 2, 2, trkHeroStats.Value / 10, 1.0, chkHeroStatMin.Checked); // Starting MP
                        statAdjust(r1, byteToUse + 4, 1, trkHeroStats.Value / 10, 1.0, chkHeroStatMin.Checked); // Starting Power
                        statAdjust(r1, byteToUse + 5, 1, trkHeroStats.Value / 10, 1.0, chkHeroStatMin.Checked); // Starting Guard
                        statAdjust(r1, byteToUse + 6, 1, trkHeroStats.Value / 10, 1.0, chkHeroStatMin.Checked); // Starting Magic
                        statAdjust(r1, byteToUse + 7, 1, trkHeroStats.Value / 10, 1.0, chkHeroStatMin.Checked); // Starting Speed
                        statAdjust(r1, byteToUse + 8, 1, trkHeroGrowth.Value / 10, 0.5, chkHeroGrowthMin.Checked); // HP Boost
                        statAdjust(r1, byteToUse + 9, 1, trkHeroGrowth.Value / 10, 0.5, chkHeroGrowthMin.Checked); // MP Boost
                        statAdjust(r1, byteToUse + 10, 1, trkHeroGrowth.Value / 10, 0.5, chkHeroGrowthMin.Checked); // Power Boost
                        statAdjust(r1, byteToUse + 11, 1, trkHeroGrowth.Value / 10, 0.5, chkHeroGrowthMin.Checked); // Guard Boost
                        statAdjust(r1, byteToUse + 12, 1, trkHeroGrowth.Value / 10, 0.5, chkHeroGrowthMin.Checked); // Magic Boost
                        statAdjust(r1, byteToUse + 13, 1, trkHeroGrowth.Value / 10, 0.5, chkHeroGrowthMin.Checked); // Speed Boost
                        statAdjust(r1, byteToUse + 17, 1, trkHeroGrowth.Value / 10, 1.0, chkHeroGrowthMin.Checked); // Starting Experience
                    }
                }
            }

            // Print stat gains
            textToHex(0x73795, "Pwr");
            textToHex(0x7379b, "Grd");
            textToHex(0x737a1, "Mgc");
            textToHex(0x737a7, "Spd");
            textToHex(0x1e643, "", new byte[] { 0x22, 0xc0, 0xfb, 0xc1, 0xea }, false);
            textToHex(0x1fbc0, "", new byte[] { 0xc2, 0x20, 0xa0, 0x08, 0x00, 0xb7, 0x53, 0x29, 0xff, 0x00, 0x85, 0x40, 0x22, 0x9f, 0x9c, 0xc1, 0xe2, 0x20, 0xa9, 0x5a, 0x8d, 0x0a, 0x03, 0xa5, 0x44, 0x8d, 0x0b, 0x03, 0xa5, 0x47, 0x8d, 0x0c, 0x03,
                                                0xc2, 0x20, 0xa0, 0x09, 0x00, 0xb7, 0x53, 0x29, 0xff, 0x00, 0x85, 0x40, 0x22, 0x9f, 0x9c, 0xc1, 0xe2, 0x20, 0xa9, 0x5a, 0x8d, 0x1a, 0x03, 0xa5, 0x44, 0x8d, 0x1b, 0x03, 0xa5, 0x47, 0x8d, 0x1c, 0x03,
                                                0xc2, 0x20, 0xa0, 0x0a, 0x00, 0xb7, 0x53, 0x29, 0xff, 0x00, 0x85, 0x40, 0x22, 0x9f, 0x9c, 0xc1, 0xe2, 0x20, 0xa9, 0x5a, 0x8d, 0x2a, 0x03, 0xa5, 0x44, 0x8d, 0x2b, 0x03, 0xa5, 0x47, 0x8d, 0x2c, 0x03,
                                                0xc2, 0x20, 0xa0, 0x0b, 0x00, 0xb7, 0x53, 0x29, 0xff, 0x00, 0x85, 0x40, 0x22, 0x9f, 0x9c, 0xc1, 0xe2, 0x20, 0xa9, 0x5a, 0x8d, 0x3a, 0x03, 0xa5, 0x44, 0x8d, 0x3b, 0x03, 0xa5, 0x47, 0x8d, 0x3c, 0x03,
                                                0xc2, 0x20, 0xa0, 0x0c, 0x00, 0xb7, 0x53, 0x29, 0xff, 0x00, 0x85, 0x40, 0x22, 0x9f, 0x9c, 0xc1, 0xe2, 0x20, 0xa9, 0x5a, 0x8d, 0x4a, 0x03, 0xa5, 0x44, 0x8d, 0x4b, 0x03, 0xa5, 0x47, 0x8d, 0x4c, 0x03,
                                                0xc2, 0x20, 0xa0, 0x0d, 0x00, 0xb7, 0x53, 0x29, 0xff, 0x00, 0x85, 0x40, 0x22, 0x9f, 0x9c, 0xc1, 0xe2, 0x20, 0xa9, 0x5a, 0x8d, 0x5a, 0x03, 0xa5, 0x44, 0x8d, 0x5b, 0x03, 0xa5, 0x47, 0x8d, 0x5c, 0x03,
                                                0xc2, 0x20, 0xa0, 0x0e, 0x00, 0xb7, 0x53, 0x6b }, false);

            romData[0x1e593] = 0x07;
            romData[0x1e598] = 0x08;
            romData[0x1e59d] = 0x09;
            romData[0x1e5b1] = 0x17;
            romData[0x1e5b6] = 0x18;
            romData[0x1e5bb] = 0x19;
            romData[0x1e5d2] = 0x27;
            romData[0x1e5d7] = 0x28;
            romData[0x1e5dc] = 0x29;
            romData[0x1e5f3] = 0x37;
            romData[0x1e5f8] = 0x38;
            romData[0x1e5fd] = 0x39;
            romData[0x1e614] = 0x47;
            romData[0x1e619] = 0x48;
            romData[0x1e61e] = 0x49;
            romData[0x1e635] = 0x57;
            romData[0x1e63a] = 0x58;
            romData[0x1e63f] = 0x59;

        }

        private void statAdjust(Random r1, int byteToUse, int bytes, double scale, double adjustment, bool min100 = false, int max = 0)
        {
            if (max == 0) max = (bytes == 2 ? 65500 : 255);
            if (bytes == 2)
            {
                int stat = romData[byteToUse] + (256 * romData[byteToUse + 1]);
                if (stat != 0)
                    stat = ScaleValue(stat, scale, adjustment, r1, min100);
                if (stat > max) stat = max;
                romData[byteToUse] = (byte)(stat % 256);
                romData[byteToUse + 1] = (byte)(stat / 256);
            } else
            {
                int stat = romData[byteToUse];
                if (stat != 0)
                    stat = ScaleValue(stat, scale, adjustment, r1, min100);
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

        private void button1_Click(object sender, EventArgs e)
        {
            loadRom();
            using (StreamWriter writer = new StreamWriter("c:\\bizhawk\\7th test\\text.txt")) {
                bool startLine = true;
                string str = "";
                for (int lnI = 0x60000; lnI <= 0x7545f; lnI++)
                {
                    if (startLine)
                    {
                        writer.WriteLine("------------------------------");
                        writer.WriteLine(lnI.ToString("X5"));
                        str = "";
                        startLine = false;
                    }
                    byte ch = romData[lnI];
                    if (ch >= 0 && ch <= 9)
                        str += (char)(ch + 48);
                    else if (ch == 0x0d)
                        str += " ";
                    else if (ch >= 0x20 && ch <= 0x39)
                        str += (char)(ch + 33);
                    else if (ch >= 0x3a && ch <= 0x53)
                        str += (char)(ch + 39);
                    else if (ch == 0x56)
                        str += "?";
                    else if (ch == 0x57)
                        str += "1";
                    else if (ch == 0x58)
                        str += "2";
                    else if (ch == 0x59)
                        str += "3";
                    else if (ch == 0x5a)
                        str += ":";
                    else if (ch == 0x5b)
                        str += ";";
                    else if (ch == 0x66)
                        str += "'";
                    else if (ch == 0x67)
                        str += "\"";
                    else if (ch == 0x68)
                        str += "-";
                    else if (ch == 0x69)
                        str += ",";
                    else if (ch == 0x6a)
                        str += ".";
                    else if (ch == 0x6b)
                        str += "HT";
                    else if (ch == 0x6c)
                        str += "SB";
                    else if (ch == 0x6d)
                        str += "CR";
                    else if (ch == 0x6e)
                        str += "MK";
                    else if (ch == 0x6f)
                        str += "HA";
                    else if (ch == 0x70)
                        str += "AX";
                    else if (ch == 0x71)
                        str += "SW";
                    else if (ch == 0x72)
                        str += "KN";
                    else if (ch == 0x73)
                        str += "ST";
                    else if (ch == 0x74)
                        str += "AR";
                    else if (ch == 0x75)
                        str += "SH";
                    else if (ch == 0x76)
                        str += "CK";
                    else if (ch == 0x7a)
                        str += "AM";
                    else if (ch == 0x7d)
                        str += "RD";
                    else if (ch == 0x7e)
                        str += "ML";
                    else if (ch == 0x7f)
                        str += "RB";
                    else if (ch == 0x82)
                        str += "<B>";
                    else if (ch == 0x85)
                        str += "!";
                    else if (ch == 0x88)
                        str += "(MC)";
                    else if (ch == 0x8b)
                        str += "(IT)";
                    else if (ch == 0x8c)
                        str += "$";
                    else if (ch == 0xc9)
                        str += "  ";
                    else if (ch == 0xca)
                        str += "er";
                    else if (ch == 0xcb)
                        str += "ar";
                    else if (ch == 0xcc)
                        str += "an";
                    else if (ch == 0xcd)
                        str += "be";
                    else if (ch == 0xce)
                        str += "re";
                    else if (ch == 0xcf)
                        str += "de";
                    else if (ch == 0xd0)
                        str += "me";
                    else if (ch == 0xd1)
                        str += "is";
                    else if (ch == 0xd2)
                        str += "if ";
                    else if (ch == 0xd3)
                        str += "ll";
                    else if (ch == 0xd4)
                        str += "se";
                    else if (ch == 0xd5)
                        str += "es";
                    else if (ch == 0xd6)
                        str += "n't";
                    else if (ch == 0xd7)
                        str += "you ";
                    else if (ch == 0xd8)
                        str += "the";
                    else if (ch == 0xd9)
                        str += "The ";
                    else if (ch == 0xda)
                        str += "it ";
                    else if (ch == 0xdb)
                        str += "It ";
                    else if (ch == 0xdc)
                        str += "to ";
                    else if (ch == 0xdd)
                        str += "We ";
                    else if (ch == 0xde)
                        str += "we";
                    else if (ch == 0xdf)
                        str += "ty";
                    else if (ch == 0xe0)
                        str += "I'm ";
                    else if (ch == 0xe1)
                        str += "have";
                    else if (ch == 0xe2)
                        str += "ble";
                    else if (ch == 0xe3)
                        str += "do";
                    else if (ch == 0xe4)
                        str += "my";
                    else if (ch == 0xe5)
                        str += "oo";
                    else if (ch == 0xe6)
                        str += "st";
                    else if (ch == 0xe7)
                        str += "ed";
                    else if (ch == 0xe8)
                        str += "on";
                    else if (ch == 0xe9)
                        str += "fa";
                    else if (ch == 0xea)
                        str += "y ";
                    else if (ch == 0xeb)
                        str += "d ";
                    else if (ch == 0xec)
                        str += "n ";
                    else if (ch == 0xed)
                        str += "wh";
                    else if (ch == 0xee)
                        str += "in";
                    else if (ch == 0xef)
                        str += " <B>";
                    else if (ch == 0xf3)
                    {
                        str = "F3";
                        writer.WriteLine(str); str = "";
                    }
                    else if (ch == 0xf6)
                    {
                        str = "SUB ROUTINE: ";
                        lnI++;
                        ch = romData[lnI];
                        str += string.Format("{0:x2} ", ch);
                        writer.WriteLine(str); str = "";
                    }
                    else if (ch == 0xf7)
                        { writer.WriteLine(str); startLine = true; }
                    else if (ch == 0xf9)
                        { writer.WriteLine(str); str = ""; }
                    else if (ch == 0xfa)
                        { writer.WriteLine(str + ">>>"); str = ""; }
                    else if (ch == 0xfb)
                    {
                        str = "(FB) TRIG-IF: ";
                        lnI++;
                        ch = romData[lnI];
                        str += string.Format("{0:x2} ", ch);
                        lnI++;
                        ch = romData[lnI];
                        str += string.Format("{0:x2} ", ch);
                        str += "GOTO ";
                        lnI++;
                        ch = romData[lnI];
                        str += string.Format("{0:x2} ", ch);
                        lnI++;
                        ch = romData[lnI];
                        str += string.Format("{0:x2} ", ch);
                        lnI++;
                        ch = romData[lnI];
                        str += string.Format("{0:x2} ", ch);
                        writer.WriteLine(str); str = "";
                    }
                    else if (ch == 0xfc)
                    {
                        str = "(FC) CHAR-IF: ";
                        lnI++;
                        ch = romData[lnI];
                        str += string.Format("{0:x2} ", ch);
                        lnI++;
                        ch = romData[lnI];
                        str += string.Format("{0:x2} ", ch);
                        str += "GOTO ";
                        lnI++;
                        ch = romData[lnI];
                        str += string.Format("{0:x2} ", ch);
                        lnI++;
                        ch = romData[lnI];
                        str += string.Format("{0:x2} ", ch);
                        lnI++;
                        ch = romData[lnI];
                        str += string.Format("{0:x2} ", ch);
                        writer.WriteLine(str); str = "";
                    }
                    else if (ch == 0xfd)
                    {
                        str = "(FD) TRIG-SET: ";
                        lnI++;
                        ch = romData[lnI];
                        str += string.Format("{0:x2} ", ch);
                        lnI++;
                        ch = romData[lnI];
                        str += string.Format("{0:x2} ", ch);
                        writer.WriteLine(str); str = "";
                    }
                    else if (ch == 0xfe)
                    {
                        str = "(FE) CHAR-SET: ";
                        lnI++;
                        ch = romData[lnI];
                        str += string.Format("{0:x2} ", ch);
                        lnI++;
                        ch = romData[lnI];
                        str += string.Format("{0:x2} ", ch);
                        writer.WriteLine(str); str = "";
                    }
                    else if (ch == 0xff)
                    {
                        str = "(FF) GOTO: ";
                        lnI++;
                        ch = romData[lnI];
                        str += string.Format("{0:x2} ", ch);
                        lnI++;
                        ch = romData[lnI];
                        str += string.Format("{0:x2} ", ch);
                        lnI++;
                        ch = romData[lnI];
                        str += string.Format("{0:x2} ", ch);
                        writer.WriteLine(str); str = "";
                    }
                }
            }
        }

        private void cmdGuide_Click(object sender, EventArgs e)
        {
            randomize();

            StreamWriter writer = File.CreateText(Path.Combine(Path.GetDirectoryName(txtFileName.Text), "7thSaga_" + txtSeed.Text + "_" + txtFlags.Text + "_HeroGuide.txt"));

            writer.WriteLine("".PadRight(20) + "Kamil".PadLeft(10) + "Olvan".PadLeft(10) + "Esuna".PadLeft(10) + "Wilme".PadLeft(10) + "Lux".PadLeft(10) + "Valsu".PadLeft(10) + "Lejes".PadLeft(10));
            int byteToUse = 0x623f;
            writer = printStats(writer, byteToUse, "Start HP");
            writer = printStats(writer, byteToUse + 2, "Start MP");
            writer = printStats(writer, byteToUse + 4, "Start PWR");
            writer = printStats(writer, byteToUse + 5, "Start GRD");
            writer = printStats(writer, byteToUse + 6, "Start MAG");
            writer = printStats(writer, byteToUse + 7, "Start SPD");

            writer = printStats(writer, byteToUse + 8, "Growth HP");
            writer = printStats(writer, byteToUse + 9, "Growth MP");
            writer = printStats(writer, byteToUse + 10, "Growth PWR");
            writer = printStats(writer, byteToUse + 11, "Growth GRD");
            writer = printStats(writer, byteToUse + 12, "Growth MAG");
            writer = printStats(writer, byteToUse + 13, "Growth SPD");

            writer.WriteLine();
            writer.WriteLine("WEAPONS");
            for (int lnJ = 1; lnJ < 51; lnJ++)
                printWeapons(writer, 0x639d + (10 * lnJ), lnJ);

            writer.WriteLine();
            writer.WriteLine("ARMOR");
            for (int lnJ = 1; lnJ < 53; lnJ++)
                printArmor(writer, 0x659b + (17 * lnJ), lnJ);

            //for (int lnI = 0; lnI < 7; lnI++)
            //{
            //    int byteToUse = 0x623f + (18 * lnI);
            //    writer.WriteLine(lnI == 0 ? "Kamil" : lnI == 1 ? "Olvan" : lnI == 2 ? "Esuna" : lnI == 3 ? "Wilme" : lnI == 4 ? "Lux" : lnI == 5 ? "Valsu" : "Lejes");
            //    writer.WriteLine("Start:   HP:  " + romData[byteToUse] + "  MP:  " + romData[byteToUse + 2] + "  PWR:  " + romData[byteToUse + 4] + "  GRD:  " + romData[byteToUse + 5] + "  MAG:  " + romData[byteToUse + 6] + "  SPD:  " + romData[byteToUse + 7]);
            //    writer.WriteLine("Growth:  HP:  " + romData[byteToUse + 8] + "  MP:  " + romData[byteToUse + 9] + "  PWR:  " + romData[byteToUse + 10] + "  GRD:  " + romData[byteToUse + 11] + "  MAG:  " + romData[byteToUse + 12] + "  SPD:  " + romData[byteToUse + 13]);

            //    writer.WriteLine("");
            //    writer.WriteLine("Weapons: (>= 50 attack)");
            //    for (int lnJ = 1; lnJ < 51; lnJ++)
            //    {
            //        byteToUse = 0x639d + (10 * lnJ);
            //        string name = (lnJ == 1 ? "SW Tranq" : lnJ == 2 ? "SW Psyte" : lnJ == 3 ? "SW Anim" : lnJ == 4 ? "SW Kryn" : lnJ == 5 ? "SW Anger" : lnJ == 6 ? "SW Natr" : lnJ == 7 ? "SW Brill" : lnJ == 8 ? "SW Cour" : lnJ == 9 ? "SW Desp" : lnJ == 10 ? "SW Fear" :
            //            lnJ == 11 ? "SW Fire" : lnJ == 12 ? "SW Insa" : lnJ == 13 ? "SW Vict" : lnJ == 14 ? "SW Ansc" : lnJ == 15 ? "SW Doom" : lnJ == 16 ? "SW Fort" : lnJ == 19 ? "SW Tidal" : lnJ == 20 ? "SW Znte" :
            //            lnJ == 21 ? "SW Mura" : lnJ == 22 ? "KN Lght" : lnJ == 23 ? "SB Saber" : lnJ == 24 ? "KN Fire" : lnJ == 25 ? "Claw" : lnJ == 26 ? "HA Znte" : lnJ == 27 ? "HA Kryn" : lnJ == 28 ? "AX Fire" : lnJ == 29 ? "AX Psyte" : lnJ == 30 ? "AX Anim" :
            //            lnJ == 31 ? "AX Anger" : lnJ == 32 ? "AX Power" : lnJ == 33 ? "AX Desp" : lnJ == 34 ? "AX Kryn" : lnJ == 35 ? "AX Fear" : lnJ == 36 ? "AX Myst" : lnJ == 37 ? "AX Hope" : lnJ == 38 ? "AX Immo" : lnJ == 39 ? "SW Sword" :
            //            lnJ == 41 ? "ST Lght" : lnJ == 42 ? "ST Petr" : lnJ == 43 ? "RD Tide" : lnJ == 44 ? "RD Conf" : lnJ == 45 ? "RD Brill" : lnJ == 46 ? "RD Desp" : lnJ == 47 ? "RD Natr" : lnJ == 48 ? "RD Fear" : lnJ == 49 ? "RD Myst" : "RD Immo");

            //        int power = romData[byteToUse] + (romData[byteToUse + 1] * 256);
            //        if (name != "" && romData[byteToUse + 4] % Math.Pow(2, lnI + 1) >= Math.Pow(2, lnI) && power >= 50)
            //            writer.WriteLine(name.PadRight(10) + " - " + power.ToString());
            //    }

            //    writer.WriteLine("");
            //    writer.WriteLine("Armor: (>= 40 defense)");
            //    for (int lnJ = 0; lnJ < 53; lnJ++)
            //    {
            //        byteToUse = 0x659b + (17 * lnJ);
            //        string name = (lnJ == 0 ? "AR Xtri" : lnJ == 1 ? "AR Psyt" : lnJ == 2 ? "AR Anim" : lnJ == 3 ? "AR Royl" : lnJ == 4 ? "AR Cour" : lnJ == 5 ? "AR Brav" : lnJ == 6 ? "AR Mystc" : lnJ == 7 ? "AR Fort" :
            //            lnJ == 8 ? "ML Scale" : lnJ == 9 ? "ML Chain" : lnJ == 10 ? "ML Kryn" : lnJ == 11 ? "CK Fire" : lnJ == 12 ? "CK Ice" : lnJ == 13 ? "RB Lght" : lnJ == 14 ? "RB Xtre" : lnJ == 15 ? "Xtri" :
            //            lnJ == 16 ? "Coat" : lnJ == 17 ? "Blck" : lnJ == 18 ? "RB Cttn" : lnJ == 19 ? "RB Silk" : lnJ == 20 ? "RB Seas" : lnJ == 21 ? "RB Hope" : lnJ == 22 ? "RB Anger" : lnJ == 23 ? "RB Vict" :
            //            lnJ == 24 ? "RB Desp" : lnJ == 25 ? "RB Conf" : lnJ == 26 ? "RB Myst" : lnJ == 27 ? "RB Immo" : lnJ == 28 ? "Brwn" : lnJ == 30 ? "SH Xtri" : lnJ == 31 ? "SH Kryn" :
            //            lnJ == 32 ? "SH Cour" : lnJ == 33 ? "SH Brill" : lnJ == 34 ? "SH Just" : lnJ == 35 ? "SH Sound" : lnJ == 36 ? "SH Myst" : lnJ == 37 ? "SH Anger" : lnJ == 38 ? "SH Mystc" : lnJ == 39 ? "SH Frtn" :
            //            lnJ == 40 ? "SH Immo" : lnJ == 41 ? "SH Illus" : lnJ == 42 ? "Amulet" : lnJ == 43 ? "Ring" : lnJ == 46 ? "Horn" : lnJ == 47 ? "Pod" :
            //            lnJ == 48 ? "HT Xtri" : lnJ == 50 ? "Scarf" : lnJ == 51 ? "MK Mask" : lnJ == 52 ? "MK Kryn" : "CR Brill");

            //        int power = romData[byteToUse] + (romData[byteToUse + 1] * 256);
            //        if (name != "" && romData[byteToUse + 4] % Math.Pow(2, lnI + 1) >= Math.Pow(2, lnI) && power >= 40)
            //            writer.WriteLine(name.PadRight(10) + " - " + power.ToString());
            //    }

            //    writer.WriteLine("Spells:");
            //    byteToUse = 0x62bd + (32 * lnI);
            //    for (int lnJ = 0; lnJ < 16; lnJ++)
            //    {
            //        int spell = romData[byteToUse + lnJ];
            //        if (spell == 0) break;
            //        writer.WriteLine(romData[byteToUse + lnJ + 16].ToString() + " - " + (spell == 1 ? "FIRE 1" : spell == 2 ? "FIRE 2" : spell == 3 ? "ICE 1" : spell == 4 ? "ICE 2" : spell == 5 ? "LASER 1" : spell == 6 ? "LASER 2" : spell == 7 ? "LASER 3" : spell == 12 ? "F BIRD" : spell == 13 ? "F BALL" :
            //            spell == 14 ? "BLZRD 1" : spell == 15 ? "BLZRD 2" : spell == 16 ? "THNDER1" : spell == 17 ? "THNDER2" : spell == 21 ? "PETRIFY" : spell == 22 ? "DEFNSE1" : spell == 23 ? "DEFNSE2" : spell == 24 ? "HEAL 1" : spell == 25 ? "HEAL 2" : spell == 26 ? "HEAL 3" :
            //            spell == 27 ? "MPCTCHR" : spell == 28 ? "AGILITY" : spell == 29 ? "F SHID" : spell == 30 ? "PROTECT" : spell == 31 ? "EXIT" : spell == 33 ? "POWER" : spell == 34 ? "HPCTCHR" : spell == 35 ? "ELIXIR" : spell == 40 ? "VACUUM1" : spell == 41 ? "VACUUM2" :
            //            spell == 45 ? "PURIFY" : spell == 46 ? "REVIVE1" : "REVIVE2"));
            //    }
            //    writer.WriteLine("Weapons to equip:");


            //    writer.WriteLine("");
            //}
            writer.Close();
            writer.Dispose();

            string finalFile = Path.Combine(Path.GetDirectoryName(txtFileName.Text), "7SR_" + txtSeed.Text + "_" + txtFlags.Text + "_HeroGuide.txt");
            lblStatus.Text = "Guide complete!  (" + finalFile + ")";
        }


        private void chkHeroInteractions_Click(object sender, EventArgs e)
        {
            chkAllHeroesJoin.Checked = chkNoHeroesJoin.Checked = false;
            determineFlags(null, null);
        }

        private void chkAllHeroesJoin_Click(object sender, EventArgs e)
        {
            chkHeroInteractions.Checked = chkNoHeroesJoin.Checked = false;
            determineFlags(null, null);
        }

        private void chkNoHeroesJoin_Click(object sender, EventArgs e)
        {
            chkHeroInteractions.Checked = chkAllHeroesJoin.Checked = false;
            determineFlags(null, null);
        }

        private void chkNoSeeds_Click(object sender, EventArgs e)
        {
            chkStores.Checked = chkNoStoreItems.Checked = false;
            determineFlags(null, null);
        }

        private void chkStores_Click(object sender, EventArgs e)
        {
            chkNoSeeds.Checked = chkNoStoreItems.Checked = false;
            determineFlags(null, null);
        }

        private void chkNoStoreItems_Click(object sender, EventArgs e)
        {
            chkNoSeeds.Checked = chkStores.Checked = false;
            determineFlags(null, null);
        }

        private void chkLearnNoMagic_Click(object sender, EventArgs e)
        {
            chkSpellLearning.Checked = chkHeroSpells.Checked = false;
            determineFlags(null, null);
        }

        private void chkSpellLearning_Click(object sender, EventArgs e)
        {
            chkLearnNoMagic.Checked = chkHeroSpells.Checked = false;
            determineFlags(null, null);
        }

        private void chkHeroSpells_Click(object sender, EventArgs e)
        {
            chkLearnNoMagic.Checked = chkSpellLearning.Checked = false;
            determineFlags(null, null);
        }

        private void chkPostBoneRemote_Click(object sender, EventArgs e)
        {
            chkPostBonePandam.Checked = chkPostBoneRandom.Checked = false;
            determineFlags(null, null);
        }

        private void chkPostBonePandam_Click(object sender, EventArgs e)
        {
            chkPostBoneRemote.Checked = chkPostBoneRandom.Checked = false;
            determineFlags(null, null);
        }

        private void chkPostBoneRandom_Click(object sender, EventArgs e)
        {
            chkPostBoneRemote.Checked = chkPostBonePandam.Checked = false;
            determineFlags(null, null);
        }
    }
}
