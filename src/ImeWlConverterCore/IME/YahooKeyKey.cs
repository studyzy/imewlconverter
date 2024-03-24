/*
 *   Copyright © 2009-2020 studyzy(深蓝,曾毅)

 *   This program "IME WL Converter(深蓝词库转换)" is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.

 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.

 *   You should have received a copy of the GNU General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    ///     雅虎奇摩输入法
    /// </summary>
    [ComboBoxShow(ConstantString.YAHOO_KEYKEY, ConstantString.YAHOO_KEYKEY_C, 200)]
    public class YahooKeyKey : BaseTextImport, IWordLibraryExport, IWordLibraryTextImport
    {
        public override CodeType CodeType
        {
            get { return CodeType.Zhuyin; }
        }

        #region IWordLibraryExport 成员

        private static string END_STRING =
            @"
# What follows is the Automatic Learning database, do not remove this
<database>
7c7b5c2bfd227076f056b3ea475ff6470400010120402020f69be92ab0f5
ef5e8a1af5e96b4da6aaf43f40b04ffaef24f67573c7e9f4f299ca612199
ea3a0861e36411133495c29dec03092f63a2187e69d7bd3483e16e9c8cc4
5edf67044dd4e8df319c63be0330279f1b80dfe0343226322267fced66d3
cd853e81bdbef322614d111cad3b4d31c2dde1f96cfe032695e0ccea6e7e
ffe4ac911eb4de113fa6dacb6dfba71dcdd2f38f748add967b501b5513d0
7f6c627f7ff32c3c456fbd2d76736a2cc78a908de8b7083040edfc868b94
29cde0bf15955987ff6039b80e2e2ab46e2ca7ae94cbc88878cecc5958ef
d09c06b1dd66267f2fda20240581ad6bd86784e5226c38a617180b2a2dd0
038738753123bb27d146acb2671cff1dc29503322a66d9cbf86ec7694d29
3f2ef3fafd11e2acae9fa29834501a62b2b930479051945d92dca1fd7660
87b74ed32162a17e10d1462ed7c017573523ced9267eaec0b445afe18e6a
3c6eb51778266113071cbb2a430d4f4c33e57e1d9bac5e39115eba114814
acad3b5e575c5f40d51459e5de51af6c6f394f05b93f90e2560ecfd56158
ce0a95c8c8ec0cdd2dcaacf2f9ba099e32c07f8fe9dba6678139fcca2a47
1108dabf0458f077137d2208b9b8e6e5dd666d69ba847633a66c84d2964c
565474c9eaf6d40df8b12533a63a60234622faffb22acf3d50a25aa42475
f4f95b00c7269ad49d28c8e17f4a3a5ab14a2bebd739fded047cd3643a05
f62a01bede74bc54ba30e5bef389c1b63f91a0825f07f0dc8676c798bbf8
768b89f32b8dae773c38879faba9f9670297353b1cdb4bd0b143d749cb96
972247dc25bc1f7dd54a2c8b130189848d9ced852022943a503b96431dde
3bfd8e52fd73167fa8ebebb8bf4d52c93d4edf44ec9e59268af375a8a576
60f07918967b2138f0f78a2f0f600cca123d978aad0cf0b02d414e1c46da
62048305d8c218fd3836f18adcf6ebd0bcf915318aad5d42c409cfd8ed6e
cf97b77c7db643f6ec988648ad7195c9b5eb2ac94faf2e47c2a1f7855667
7466baa76d578012a9dd365bbf0429fa8f0a978934bb5e9743fa751980c7
95d93942025092120a8d2a76ca5c7e1399e97d1a590f332c98d6bb512b81
93482cc4b6ad7c230a0c8dff4556574c0b80479753327df0e073bf311f2b
b954c341979636368f2a9a28dfafae6d98bd92c4ee8596c68321d696521b
5824d7548d40cc33d8ec87cde4f7a5d06fd51bfe724f5883cc60595bd82b
ba030eb80731637a17a9fc2829d8f45a485890af18f3318e7038c5ad6ad1
91f2955e07b9841ce98d318fc715aa8c80f643c299ae1932731a9d2ee725
bdb1b4c398bf723d03579b1d4fe78cd6d99e74ffed144269fda981124290
e4331f765565f6ab42a49686645bf879a1cce6b4c8c0ba5d2095d2ec3497
7736b89827b9d9d7669ddfa3def472782673fe132560ea1cfc4cb0df2a0c
0e16d63adbe2820a3580e30560b0448b1b1f0feae32ee2e27a102bcd71e1
a6d0efb1bbb4ceb030268cd73fb5f9a737f5d80c52b00c5bd89720202e1a
1c1a3a1464fbe2405a0e2ad5a1abcb7bf55c74e0d01f605af778755fd103
54dc05190f42e6c2d7b042b64a9e14af0603185a976d369f183c61fc2edb
b5d7fcbd0a1ab528979bb028e1b6a1add1ad460d4037a54ece00c4181221
c3ff42c148f846a37a36ee875fafc12458d95c52ac05bc8ed09992afac68
c24e7bae56be6c022c0dafc9e435a101bd200b3fb86f086c1288e879f0f1
e50aa292ea603c50a9f2443c20b94868c031d85ee48e9c49b5930dfcffea
cc9c77122a6469af05b62d6bdd8f7533ffda2a1847aec60d2cf563e44233
7b1a6305e44c0ff8e497e1b4b9e481ee9292c311f483c6100012360e32a6
09bde8339d4925da995f8b441bd9f788d04927638b88a040368eba83b3de
10fc84253011ce8fba5f3a52198c349be3c45761bf3b3555fcc089e732cf
93e257f3055c3200fc3511913583bc08dc0dd4bc52863e08a5d148c68f7a
39a2994c632ec931b90e70123a859c8d35efdc562795ba8d1e343c4916a6
2fea08a9296f9e5930c07e4a162b396c134b62cc24415524fe6f34feb555
43a02e900f978e2b9190c8f8e807973ecf1e9d23894d1f45a172e7f4a838
3dd5912c65e00cb73142ee0f574a2af8eed0e21b2aacf2df1ce06f8764e0
c1b72c18ff83709a007237a0e57daa4cd888342c6a2d5ac223a2b432626b
3a40e67231cd4d14b241dc900f8192409a7919bf0e3119e92c051a34dd84
ede9b3df1593617d3ed08c27e9cd5ffcd3c86a7cb46d4f995d8dc528457e
03bd448b4ccd2104cfe93b61aa22ab489d0d4b5b8f5b17d9b6ace996eb54
4bf0949f55de945a118c437302e3654918f7e12872807234e0510c3e5569
ccb4353cfdb4ecf50b32d35c9590e70b635a52f7741c150829de673ecfe7
a89dcfc838394d0611fa123700c3dccaba6be0c85f9edbd60d13fc8afd86
3e355b184223ba56edfcf08a905643ae24aabea23c601c5dcd175b698d2f
eb27b765dc9187968273129cb133cf611ef7594c388c0ae4cb7f8c37bc1e
15de5ac3cb54c61133dbe0648e9db1867e692d46229b6e4dad81a9cd5647
cb4c4b9ec18e081c65994604cf3a82c310efd8622c07b26d14e5ea5c5d19
206f78fe3a836d2edbd3ab25c9a34059c7eafa523f94213cbd60de7bbe4c
b378fa811e414ac20d26976f03c4bc31746b39e1acf6bf3de00b88514801
a5b559b38f621be00a2d54aebdbd840589963093bdc95f0ae4e6aa88d830
1de5a21c1d9970c5260ab4e084bff05b762e951f1b3406e55ff0f624e39e
b6b828e4bccb60fe2ea48f469acd30cf35fe38d5d16d53667d3fd20ffc18
43fb9e6755b8fd16219312c98ed0e18ab2daeef24cf2711b404511eb4318
f13008112a549706988d64482ab5b35a714f6bfaee5771042417fe463579
f558c414527243db8c3b2b0f38fd11a8913edacb869746f0a026325ffdf5
b3343786f91b1ce1336b52f3982282fc31bffdb6f25999134488833538e4
ab979a125f63a7e5a2287bee1944cc85d74c04959ceefbf97493e50d630c
dc1df6c167002abb513c0f75253dfd73835689898a900c07e2ebe1b31889
ccc8d94d80f36e48f26c652fcc2c2fb069d914edb7b603e27e2c573e5c6b
ef2ceab1a16246834d66b9ed18513ac1d77ece3edbef64d87c5bcbd64ca8
ac4d9777766a1d78e33ca23fbe02c796c9f88a4f1df77ab79f4cf495baab
08da38b110f22417dad94b7ecf1c3a375b57b370ee9dd0b1e30147e08755
3c984e1bdeb731adacf385e841004bc08be3cc665c2845da1089880aac59
def3b84687837c747bce51bb8e0eaaf814cfb949f698f852592f9e22ff85
593512a3c9d19d87df6b64f380f44401e157e442645eb4cd3ff33a171c3e
dcc9bb168308628cd2b0819f94e130833b84c4b0f14386e5725aac137474
c407ebe18709b78b3d4c5af00c15b8f6026b97e8cedf613700ea2159b3b8
dc5b3c0c45391d8e9baf0ec87125abe6dbe545ea4234c2b7f8fcb701c21d
2d17597cf73c205754f47e99f978138078d73fd8449cddd62efcf7fd3a74
8e7317072ce87d3809d8a0e7daf4f6090e6000d2f1f8bdc7fa583fb86111
45b65afa1b16ebfbd98825142fa26e9a10e14cc29ea3ec2b5496ff895b2c
b45ec3933ff0fc02fde629ea1b8d013944e7f45f457c27629dd38e108b20
34ad8443439a164f907e689b309001a1b92415abba639dd6ef66021aa1d6
b60c10e7e0c54b4c4d2515fe5ad7cec9b113dfaa22b0aaf3f453b89cdf25
7d83f6e70fcb0005300acd00e56146bda6ad618da9a3571e27b169cb41b1
92e3b330500a7807c181980311e0fb41e6eb6786a11db3734afcf9313f86
d64234d2489e89756e782506647efaf83761bf7d7b39147857747d1c92f3
0c54752377dab88dc905aad62442ef1b28a70070dcb4e28b625db0b0834f
4eee7f6a10d87d714e9fe7ebef0e1864168411eff852c3196ee186a096aa
2d7a1a965bdfdf8739c8a4c8d6ba1c52b49c2297e277c4b51997cfa51d9b
4a88a61ffd9626eb1d82d667ff16724c5624e13087b30a0a98712c81ddce
6643bb91d907b17b056507174c327a1e414a8f90081f08a055776295bb73
5eb57ad7ddd3ea7e8995f9180d17aca2da6a3037ed226a3d036bbc90f1b6
902abddcb072f1964753b305bbc3298343c6f5f345443a363853abbc7a1c
8b31a4bf589ec331d714a00ed0f885b7beee001932ae4a9e16f2178bbed0
b348405ef9a3aebf9328958712e2d0048e97e51bd7e2ab633571cbc51f86
4ec63bf0b064eaff58fc9805
</database>
";

        //private readonly IWordCodeGenerater generater = new ZhuyinGenerater();

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        public string ExportLine(WordLibrary wl)
        {
            var sb = new StringBuilder();

            sb.Append(wl.Word);
            sb.Append("\t");
            //IList<string> zhuyins = null;
            //if (wl.CodeType == CodeType.Pinyin) //如果本来就是拼音输入法导入的，那么就用其拼音，不过得加上音调
            //{
            //    IList<string> pinyin = new List<string>();
            //    for (int i = 0; i < wl.PinYin.Length; i++)
            //    {
            //        if (regex.IsMatch(wl.PinYin[i]))
            //        {
            //            pinyin.Add(wl.PinYin[i]);
            //        }
            //        else
            //        {
            //            pinyin.Add(PinyinHelper.AddToneToPinyin(wl.Word[i], wl.PinYin[i]));
            //        }
            //    }
            //    zhuyins = ZhuyinHelper.GetZhuyin(pinyin);
            //}
            //else
            //{
            //    //zhuyins = generater.GetCodeOfString(wl.Word);
            //}

            //sb.Append(CollectionHelper.ListToString(zhuyins, ","));

            sb.Append(wl.GetPinYinString(",", BuildType.None));
            sb.Append("\t");
            sb.Append("-1.0");
            sb.Append("\t");
            sb.Append("0.0");
            return sb.ToString();
        }

        public IList<string> Export(WordLibraryList wlList)
        {
            var sb = new StringBuilder();
            sb.Append("MJSR version 1.0.0\r\n");
            for (int i = 0; i < wlList.Count; i++)
            {
                try
                {
                    sb.Append(ExportLine(wlList[i]));
                    sb.Append("\r\n");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(wlList[i] + ex.Message);
                }
            }
            sb.Append(END_STRING);
            return new List<string>() { sb.ToString() };
        }

        #endregion

        #region IWordLibraryImport 成员




        public override WordLibraryList ImportLine(string line)
        {
            string[] c = line.Split('\t');
            var wl = new WordLibrary();
            wl.Word = c[0];
            wl.Rank = DefaultRank;
            string zhuyin = c[1];
            var pys = new List<string>();
            foreach (string zy in zhuyin.Split(','))
            {
                try
                {
                    string py = ZhuyinHelper.GetPinyin(zy);
                    pys.Add(py);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            wl.PinYin = pys.ToArray();
            var wll = new WordLibraryList();
            wll.Add(wl);
            return wll;
        }

        protected override bool IsContent(string line)
        {
            return line.Split('\t').Length == 4;
        }

        #endregion

        //private static readonly Regex regex = new Regex(@"^[a-zA-Z]+\d$");
    }
}
