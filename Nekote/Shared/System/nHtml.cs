using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public static class nHtml
    {
        // 内部的には、HttpUtility → HttpEncoder → WebUtility となっている
        // 他には SecurityElement があるが、これは XML 用なのか、< > " ' & の5文字のみ扱う

        // アポストロフィーの扱いが興味深い
        // 確か、古い HTML ではエンコードが不要だった
        // そのうち現れた XML では &apos; への変換が必要とされた
        // その後、HTML は XHTML であるなら XML のサブセットと位置付けられたときに HTML でも ' の変換が求められた
        // 今どうなっているかと言えば、古いブラウザーが &apos; を認識しないことが考えられるからか、WebUtility は ' を &#39; にするようだ
        // &apos; がなかった時代があったのだから、&apos; でも &#39; でもよいところで後者を選ぶのは理にかなう

        // HttpUtility.cs
        // https://source.dot.net/#System.Web.HttpUtility/System/Web/HttpUtility.cs

        // HttpEncoder.cs
        // https://source.dot.net/#System.Web.HttpUtility/System/Web/Util/HttpEncoder.cs

        // WebUtility.cs
        // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Net/WebUtility.cs

        // SecurityElement.cs
        // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Security/SecurityElement.cs

        public static string? Encode (string? value)
        {
            return WebUtility.HtmlEncode (value);
        }

        public static string? Decode (string? value)
        {
            return WebUtility.HtmlDecode (value);
        }
    }
}
