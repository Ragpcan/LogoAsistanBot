<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="2.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:cac="urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"
    xmlns:cbc="urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"
    xmlns:ccts="urn:un:unece:uncefact:documentation:2"
    xmlns:clm54217="urn:un:unece:uncefact:codelist:specification:54217:2001"
    xmlns:clm5639="urn:un:unece:uncefact:codelist:specification:5639:1988"
    xmlns:clm66411="urn:un:unece:uncefact:codelist:specification:66411:2001"
    xmlns:clmIANAMIMEMediaType="urn:un:unece:uncefact:codelist:specification:IANAMIMEMediaType:2003"
    xmlns:fn="http://www.w3.org/2005/xpath-functions"
    xmlns:link="http://www.xbrl.org/2003/linkbase"
    xmlns:n1="urn:oasis:names:specification:ubl:schema:xsd:DespatchAdvice-2"
    xmlns:qdt="urn:oasis:names:specification:ubl:schema:xsd:QualifiedDatatypes-2"
    xmlns:udt="urn:un:unece:uncefact:data:specification:UnqualifiedDataTypesSchemaModule:2"
    xmlns:xbrldi="http://xbrl.org/2006/xbrldi"
    xmlns:xbrli="http://www.xbrl.org/2003/instance"
    xmlns:xdt="http://www.w3.org/2005/xpath-datatypes"
    xmlns:xlink="http://www.w3.org/1999/xlink"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:xsd="http://www.w3.org/2001/XMLSchema"
    xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
    exclude-result-prefixes="cac cbc ccts clm54217 clm5639 clm66411 clmIANAMIMEMediaType fn link n1 qdt udt xbrldi xbrli xdt xlink xs xsd xsi">
    <xsl:character-map name="a">
        <xsl:output-character character="&#128;" string=""/>
        <xsl:output-character character="&#129;" string=""/>
        <xsl:output-character character="&#130;" string=""/>
        <xsl:output-character character="&#131;" string=""/>
        <xsl:output-character character="&#132;" string=""/>
        <xsl:output-character character="&#133;" string=""/>
        <xsl:output-character character="&#134;" string=""/>
        <xsl:output-character character="&#135;" string=""/>
        <xsl:output-character character="&#136;" string=""/>
        <xsl:output-character character="&#137;" string=""/>
        <xsl:output-character character="&#138;" string=""/>
        <xsl:output-character character="&#139;" string=""/>
        <xsl:output-character character="&#140;" string=""/>
        <xsl:output-character character="&#141;" string=""/>
        <xsl:output-character character="&#142;" string=""/>
        <xsl:output-character character="&#143;" string=""/>
        <xsl:output-character character="&#144;" string=""/>
        <xsl:output-character character="&#145;" string=""/>
        <xsl:output-character character="&#146;" string=""/>
        <xsl:output-character character="&#147;" string=""/>
        <xsl:output-character character="&#148;" string=""/>
        <xsl:output-character character="&#149;" string=""/>
        <xsl:output-character character="&#150;" string=""/>
        <xsl:output-character character="&#151;" string=""/>
        <xsl:output-character character="&#152;" string=""/>
        <xsl:output-character character="&#153;" string=""/>
        <xsl:output-character character="&#154;" string=""/>
        <xsl:output-character character="&#155;" string=""/>
        <xsl:output-character character="&#156;" string=""/>
        <xsl:output-character character="&#157;" string=""/>
        <xsl:output-character character="&#158;" string=""/>
        <xsl:output-character character="&#159;" string=""/>
    </xsl:character-map>
    <xsl:decimal-format name="european" decimal-separator="," grouping-separator="." NaN=""/>
    <xsl:output version="4.0" method="html" indent="no" encoding="UTF-8"
    doctype-public="-//W3C//DTD HTML 4.01 Transitional//EN"
    doctype-system="http://www.w3.org/TR/html4/loose.dtd" use-character-maps="a"/>
    <xsl:param name="SV_OutputFormat" select="'HTML'"/>
    <xsl:variable name="XML" select="/"/>
    <xsl:key name="groupByTaxes" match="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal" use="cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode" />
    <xsl:key name="groupByTevkifat" match="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:WithholdingTaxTotal/cac:TaxSubtotal" use="cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode" />
    <xsl:variable name="CurrValue" select="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentDescription=&apos;DOVIZKURU&apos;]/cbc:DocumentTypeCode"></xsl:variable>

    <xsl:template match="/"><html>
 <head>
  <script type="text/javascript"><![CDATA[var QRCode; !function () {
                function a(a) {
                    this.mode = c.MODE_8BIT_BYTE, this.data = a, this.parsedData = [];
                    for (var b = [], d = 0, e = this.data.length; e > d; d++) {
                        var f = this.data.charCodeAt(d); f > 65536 ? (b[0] = 240 | (1835008 & f) >>> 18,
                            b[1] = 128 | (258048 & f) >>> 12, b[2] = 128 | (4032 & f) >>> 6, b[3] = 128 | 63 & f) : f > 2048 ? (b[0] = 224 | (61440 & f) >>> 12, b[1] = 128 | (4032 & f) >>> 6,
                                b[2] = 128 | 63 & f) : f > 128 ? (b[0] = 192 | (1984 & f) >>> 6,
                                    b[1] = 128 | 63 & f) : b[0] = f, this.parsedData = this.parsedData.concat(b)
                    }
                    this.parsedData.length != this.data.length && (this.parsedData.unshift(191), this.parsedData.unshift(187),
                        this.parsedData.unshift(239))
                } function b(a, b) {
                    this.typeNumber = a, this.errorCorrectLevel = b, this.modules = null, this.moduleCount = 0, this.dataCache = null,
                    this.dataList = []
                } function i(a, b) {
                    if (void 0 == a.length) throw new Error(a.length + "/" + b); for (var c = 0; c < a.length && 0 == a[c];)c++;
                    this.num = new Array(a.length - c + b); for (var d = 0; d < a.length - c; d++)this.num[d] = a[d + c]
                } function j(a, b) { this.totalCount = a, this.dataCount = b }
                function k() { this.buffer = [], this.length = 0 } function m() { return "undefined" != typeof CanvasRenderingContext2D } function n() { var a = !1, b = navigator.userAgent; return /android/i.test(b) && (a = !0, aMat = b.toString().match(/android ([0-9]\.[0-9])/i), aMat && aMat[1] && (a = parseFloat(aMat[1]))), a } function r(a, b) { for (var c = 1, e = s(a), f = 0, g = l.length; g >= f; f++) { var h = 0; switch (b) { case d.L: h = l[f][0]; break; case d.M: h = l[f][1]; break; case d.Q: h = l[f][2]; break; case d.H: h = l[f][3] }if (h >= e) break; c++ } if (c > l.length) throw new Error("Too long data"); return c } function s(a) { var b = encodeURI(a).toString().replace(/\%[0-9a-fA-F]{2}/g, "a"); return b.length + (b.length != a ? 3 : 0) } a.prototype = { getLength: function () { return this.parsedData.length }, write: function (a) { for (var b = 0, c = this.parsedData.length; c > b; b++)a.put(this.parsedData[b], 8) } }, b.prototype = { addData: function (b) { var c = new a(b); this.dataList.push(c), this.dataCache = null }, isDark: function (a, b) { if (0 > a || this.moduleCount <= a || 0 > b || this.moduleCount <= b) throw new Error(a + "," + b); return this.modules[a][b] }, getModuleCount: function () { return this.moduleCount }, make: function () { this.makeImpl(!1, this.getBestMaskPattern()) }, makeImpl: function (a, c) { this.moduleCount = 4 * this.typeNumber + 17, this.modules = new Array(this.moduleCount); for (var d = 0; d < this.moduleCount; d++) { this.modules[d] = new Array(this.moduleCount); for (var e = 0; e < this.moduleCount; e++)this.modules[d][e] = null } this.setupPositionProbePattern(0, 0), this.setupPositionProbePattern(this.moduleCount - 7, 0), this.setupPositionProbePattern(0, this.moduleCount - 7), this.setupPositionAdjustPattern(), this.setupTimingPattern(), this.setupTypeInfo(a, c), this.typeNumber >= 7 && this.setupTypeNumber(a), null == this.dataCache && (this.dataCache = b.createData(this.typeNumber, this.errorCorrectLevel, this.dataList)), this.mapData(this.dataCache, c) }, setupPositionProbePattern: function (a, b) { for (var c = -1; 7 >= c; c++)if (!(-1 >= a + c || this.moduleCount <= a + c)) for (var d = -1; 7 >= d; d++)-1 >= b + d || this.moduleCount <= b + d || (this.modules[a + c][b + d] = c >= 0 && 6 >= c && (0 == d || 6 == d) || d >= 0 && 6 >= d && (0 == c || 6 == c) || c >= 2 && 4 >= c && d >= 2 && 4 >= d ? !0 : !1) }, getBestMaskPattern: function () { for (var a = 0, b = 0, c = 0; 8 > c; c++) { this.makeImpl(!0, c); var d = f.getLostPoint(this); (0 == c || a > d) && (a = d, b = c) } return b }, createMovieClip: function (a, b, c) { var d = a.createEmptyMovieClip(b, c), e = 1; this.make(); for (var f = 0; f < this.modules.length; f++)for (var g = f * e, h = 0; h < this.modules[f].length; h++) { var i = h * e, j = this.modules[f][h]; j && (d.beginFill(0, 100), d.moveTo(i, g), d.lineTo(i + e, g), d.lineTo(i + e, g + e), d.lineTo(i, g + e), d.endFill()) } return d }, setupTimingPattern: function () { for (var a = 8; a < this.moduleCount - 8; a++)null == this.modules[a][6] && (this.modules[a][6] = 0 == a % 2); for (var b = 8; b < this.moduleCount - 8; b++)null == this.modules[6][b] && (this.modules[6][b] = 0 == b % 2) }, setupPositionAdjustPattern: function () { for (var a = f.getPatternPosition(this.typeNumber), b = 0; b < a.length; b++)for (var c = 0; c < a.length; c++) { var d = a[b], e = a[c]; if (null == this.modules[d][e]) for (var g = -2; 2 >= g; g++)for (var h = -2; 2 >= h; h++)this.modules[d + g][e + h] = -2 == g || 2 == g || -2 == h || 2 == h || 0 == g && 0 == h ? !0 : !1 } }, setupTypeNumber: function (a) { for (var b = f.getBCHTypeNumber(this.typeNumber), c = 0; 18 > c; c++) { var d = !a && 1 == (1 & b >> c); this.modules[Math.floor(c / 3)][c % 3 + this.moduleCount - 8 - 3] = d } for (var c = 0; 18 > c; c++) { var d = !a && 1 == (1 & b >> c); this.modules[c % 3 + this.moduleCount - 8 - 3][Math.floor(c / 3)] = d } }, setupTypeInfo: function (a, b) { for (var c = this.errorCorrectLevel << 3 | b, d = f.getBCHTypeInfo(c), e = 0; 15 > e; e++) { var g = !a && 1 == (1 & d >> e); 6 > e ? this.modules[e][8] = g : 8 > e ? this.modules[e + 1][8] = g : this.modules[this.moduleCount - 15 + e][8] = g } for (var e = 0; 15 > e; e++) { var g = !a && 1 == (1 & d >> e); 8 > e ? this.modules[8][this.moduleCount - e - 1] = g : 9 > e ? this.modules[8][15 - e - 1 + 1] = g : this.modules[8][15 - e - 1] = g } this.modules[this.moduleCount - 8][8] = !a }, mapData: function (a, b) { for (var c = -1, d = this.moduleCount - 1, e = 7, g = 0, h = this.moduleCount - 1; h > 0; h -= 2)for (6 == h && h--; ;) { for (var i = 0; 2 > i; i++)if (null == this.modules[d][h - i]) { var j = !1; g < a.length && (j = 1 == (1 & a[g] >>> e)); var k = f.getMask(b, d, h - i); k && (j = !j), this.modules[d][h - i] = j, e--, -1 == e && (g++, e = 7) } if (d += c, 0 > d || this.moduleCount <= d) { d -= c, c = -c; break } } } }, b.PAD0 = 236, b.PAD1 = 17, b.createData = function (a, c, d) { for (var e = j.getRSBlocks(a, c), g = new k, h = 0; h < d.length; h++) { var i = d[h]; g.put(i.mode, 4), g.put(i.getLength(), f.getLengthInBits(i.mode, a)), i.write(g) } for (var l = 0, h = 0; h < e.length; h++)l += e[h].dataCount; if (g.getLengthInBits() > 8 * l) throw new Error("code length overflow. (" + g.getLengthInBits() + ">" + 8 * l + ")"); for (g.getLengthInBits() + 4 <= 8 * l && g.put(0, 4); 0 != g.getLengthInBits() % 8;)g.putBit(!1); for (; ;) { if (g.getLengthInBits() >= 8 * l) break; if (g.put(b.PAD0, 8), g.getLengthInBits() >= 8 * l) break; g.put(b.PAD1, 8) } return b.createBytes(g, e) }, b.createBytes = function (a, b) { for (var c = 0, d = 0, e = 0, g = new Array(b.length), h = new Array(b.length), j = 0; j < b.length; j++) { var k = b[j].dataCount, l = b[j].totalCount - k; d = Math.max(d, k), e = Math.max(e, l), g[j] = new Array(k); for (var m = 0; m < g[j].length; m++)g[j][m] = 255 & a.buffer[m + c]; c += k; var n = f.getErrorCorrectPolynomial(l), o = new i(g[j], n.getLength() - 1), p = o.mod(n); h[j] = new Array(n.getLength() - 1); for (var m = 0; m < h[j].length; m++) { var q = m + p.getLength() - h[j].length; h[j][m] = q >= 0 ? p.get(q) : 0 } } for (var r = 0, m = 0; m < b.length; m++)r += b[m].totalCount; for (var s = new Array(r), t = 0, m = 0; d > m; m++)for (var j = 0; j < b.length; j++)m < g[j].length && (s[t++] = g[j][m]); for (var m = 0; e > m; m++)for (var j = 0; j < b.length; j++)m < h[j].length && (s[t++] = h[j][m]); return s }; for (var c = { MODE_NUMBER: 1, MODE_ALPHA_NUM: 2, MODE_8BIT_BYTE: 4, MODE_KANJI: 8 }, d = { L: 1, M: 0, Q: 3, H: 2 }, e = { PATTERN000: 0, PATTERN001: 1, PATTERN010: 2, PATTERN011: 3, PATTERN100: 4, PATTERN101: 5, PATTERN110: 6, PATTERN111: 7 }, f = { PATTERN_POSITION_TABLE: [[], [6, 18], [6, 22], [6, 26], [6, 30], [6, 34], [6, 22, 38], [6, 24, 42], [6, 26, 46], [6, 28, 50], [6, 30, 54], [6, 32, 58], [6, 34, 62], [6, 26, 46, 66], [6, 26, 48, 70], [6, 26, 50, 74], [6, 30, 54, 78], [6, 30, 56, 82], [6, 30, 58, 86], [6, 34, 62, 90], [6, 28, 50, 72, 94], [6, 26, 50, 74, 98], [6, 30, 54, 78, 102], [6, 28, 54, 80, 106], [6, 32, 58, 84, 110], [6, 30, 58, 86, 114], [6, 34, 62, 90, 118], [6, 26, 50, 74, 98, 122], [6, 30, 54, 78, 102, 126], [6, 26, 52, 78, 104, 130], [6, 30, 56, 82, 108, 134], [6, 34, 60, 86, 112, 138], [6, 30, 58, 86, 114, 142], [6, 34, 62, 90, 118, 146], [6, 30, 54, 78, 102, 126, 150], [6, 24, 50, 76, 102, 128, 154], [6, 28, 54, 80, 106, 132, 158], [6, 32, 58, 84, 110, 136, 162], [6, 26, 54, 82, 110, 138, 166], [6, 30, 58, 86, 114, 142, 170]], G15: 1335, G18: 7973, G15_MASK: 21522, getBCHTypeInfo: function (a) { for (var b = a << 10; f.getBCHDigit(b) - f.getBCHDigit(f.G15) >= 0;)b ^= f.G15 << f.getBCHDigit(b) - f.getBCHDigit(f.G15); return (a << 10 | b) ^ f.G15_MASK }, getBCHTypeNumber: function (a) { for (var b = a << 12; f.getBCHDigit(b) - f.getBCHDigit(f.G18) >= 0;)b ^= f.G18 << f.getBCHDigit(b) - f.getBCHDigit(f.G18); return a << 12 | b }, getBCHDigit: function (a) { for (var b = 0; 0 != a;)b++, a >>>= 1; return b }, getPatternPosition: function (a) { return f.PATTERN_POSITION_TABLE[a - 1] }, getMask: function (a, b, c) { switch (a) { case e.PATTERN000: return 0 == (b + c) % 2; case e.PATTERN001: return 0 == b % 2; case e.PATTERN010: return 0 == c % 3; case e.PATTERN011: return 0 == (b + c) % 3; case e.PATTERN100: return 0 == (Math.floor(b / 2) + Math.floor(c / 3)) % 2; case e.PATTERN101: return 0 == b * c % 2 + b * c % 3; case e.PATTERN110: return 0 == (b * c % 2 + b * c % 3) % 2; case e.PATTERN111: return 0 == (b * c % 3 + (b + c) % 2) % 2; default: throw new Error("bad maskPattern:" + a) } }, getErrorCorrectPolynomial: function (a) { for (var b = new i([1], 0), c = 0; a > c; c++)b = b.multiply(new i([1, g.gexp(c)], 0)); return b }, getLengthInBits: function (a, b) { if (b >= 1 && 10 > b) switch (a) { case c.MODE_NUMBER: return 10; case c.MODE_ALPHA_NUM: return 9; case c.MODE_8BIT_BYTE: return 8; case c.MODE_KANJI: return 8; default: throw new Error("mode:" + a) } else if (27 > b) switch (a) { case c.MODE_NUMBER: return 12; case c.MODE_ALPHA_NUM: return 11; case c.MODE_8BIT_BYTE: return 16; case c.MODE_KANJI: return 10; default: throw new Error("mode:" + a) } else { if (!(41 > b)) throw new Error("type:" + b); switch (a) { case c.MODE_NUMBER: return 14; case c.MODE_ALPHA_NUM: return 13; case c.MODE_8BIT_BYTE: return 16; case c.MODE_KANJI: return 12; default: throw new Error("mode:" + a) } } }, getLostPoint: function (a) { for (var b = a.getModuleCount(), c = 0, d = 0; b > d; d++)for (var e = 0; b > e; e++) { for (var f = 0, g = a.isDark(d, e), h = -1; 1 >= h; h++)if (!(0 > d + h || d + h >= b)) for (var i = -1; 1 >= i; i++)0 > e + i || e + i >= b || (0 != h || 0 != i) && g == a.isDark(d + h, e + i) && f++; f > 5 && (c += 3 + f - 5) } for (var d = 0; b - 1 > d; d++)for (var e = 0; b - 1 > e; e++) { var j = 0; a.isDark(d, e) && j++, a.isDark(d + 1, e) && j++, a.isDark(d, e + 1) && j++, a.isDark(d + 1, e + 1) && j++, (0 == j || 4 == j) && (c += 3) } for (var d = 0; b > d; d++)for (var e = 0; b - 6 > e; e++)a.isDark(d, e) && !a.isDark(d, e + 1) && a.isDark(d, e + 2) && a.isDark(d, e + 3) && a.isDark(d, e + 4) && !a.isDark(d, e + 5) && a.isDark(d, e + 6) && (c += 40); for (var e = 0; b > e; e++)for (var d = 0; b - 6 > d; d++)a.isDark(d, e) && !a.isDark(d + 1, e) && a.isDark(d + 2, e) && a.isDark(d + 3, e) && a.isDark(d + 4, e) && !a.isDark(d + 5, e) && a.isDark(d + 6, e) && (c += 40); for (var k = 0, e = 0; b > e; e++)for (var d = 0; b > d; d++)a.isDark(d, e) && k++; var l = Math.abs(100 * k / b / b - 50) / 5; return c += 10 * l } }, g = { glog: function (a) { if (1 > a) throw new Error("glog(" + a + ")"); return g.LOG_TABLE[a] }, gexp: function (a) { for (; 0 > a;)a += 255; for (; a >= 256;)a -= 255; return g.EXP_TABLE[a] }, EXP_TABLE: new Array(256), LOG_TABLE: new Array(256) }, h = 0; 8 > h; h++)g.EXP_TABLE[h] = 1 << h; for (var h = 8; 256 > h; h++)g.EXP_TABLE[h] = g.EXP_TABLE[h - 4] ^ g.EXP_TABLE[h - 5] ^ g.EXP_TABLE[h - 6] ^ g.EXP_TABLE[h - 8]; for (var h = 0; 255 > h; h++)g.LOG_TABLE[g.EXP_TABLE[h]] = h; i.prototype = { get: function (a) { return this.num[a] }, getLength: function () { return this.num.length }, multiply: function (a) { for (var b = new Array(this.getLength() + a.getLength() - 1), c = 0; c < this.getLength(); c++)for (var d = 0; d < a.getLength(); d++)b[c + d] ^= g.gexp(g.glog(this.get(c)) + g.glog(a.get(d))); return new i(b, 0) }, mod: function (a) { if (this.getLength() - a.getLength() < 0) return this; for (var b = g.glog(this.get(0)) - g.glog(a.get(0)), c = new Array(this.getLength()), d = 0; d < this.getLength(); d++)c[d] = this.get(d); for (var d = 0; d < a.getLength(); d++)c[d] ^= g.gexp(g.glog(a.get(d)) + b); return new i(c, 0).mod(a) } }, j.RS_BLOCK_TABLE = [[1, 26, 19], [1, 26, 16], [1, 26, 13], [1, 26, 9], [1, 44, 34], [1, 44, 28], [1, 44, 22], [1, 44, 16], [1, 70, 55], [1, 70, 44], [2, 35, 17], [2, 35, 13], [1, 100, 80], [2, 50, 32], [2, 50, 24], [4, 25, 9], [1, 134, 108], [2, 67, 43], [2, 33, 15, 2, 34, 16], [2, 33, 11, 2, 34, 12], [2, 86, 68], [4, 43, 27], [4, 43, 19], [4, 43, 15], [2, 98, 78], [4, 49, 31], [2, 32, 14, 4, 33, 15], [4, 39, 13, 1, 40, 14], [2, 121, 97], [2, 60, 38, 2, 61, 39], [4, 40, 18, 2, 41, 19], [4, 40, 14, 2, 41, 15], [2, 146, 116], [3, 58, 36, 2, 59, 37], [4, 36, 16, 4, 37, 17], [4, 36, 12, 4, 37, 13], [2, 86, 68, 2, 87, 69], [4, 69, 43, 1, 70, 44], [6, 43, 19, 2, 44, 20], [6, 43, 15, 2, 44, 16], [4, 101, 81], [1, 80, 50, 4, 81, 51], [4, 50, 22, 4, 51, 23], [3, 36, 12, 8, 37, 13], [2, 116, 92, 2, 117, 93], [6, 58, 36, 2, 59, 37], [4, 46, 20, 6, 47, 21], [7, 42, 14, 4, 43, 15], [4, 133, 107], [8, 59, 37, 1, 60, 38], [8, 44, 20, 4, 45, 21], [12, 33, 11, 4, 34, 12], [3, 145, 115, 1, 146, 116], [4, 64, 40, 5, 65, 41], [11, 36, 16, 5, 37, 17], [11, 36, 12, 5, 37, 13], [5, 109, 87, 1, 110, 88], [5, 65, 41, 5, 66, 42], [5, 54, 24, 7, 55, 25], [11, 36, 12], [5, 122, 98, 1, 123, 99], [7, 73, 45, 3, 74, 46], [15, 43, 19, 2, 44, 20], [3, 45, 15, 13, 46, 16], [1, 135, 107, 5, 136, 108], [10, 74, 46, 1, 75, 47], [1, 50, 22, 15, 51, 23], [2, 42, 14, 17, 43, 15], [5, 150, 120, 1, 151, 121], [9, 69, 43, 4, 70, 44], [17, 50, 22, 1, 51, 23], [2, 42, 14, 19, 43, 15], [3, 141, 113, 4, 142, 114], [3, 70, 44, 11, 71, 45], [17, 47, 21, 4, 48, 22], [9, 39, 13, 16, 40, 14], [3, 135, 107, 5, 136, 108], [3, 67, 41, 13, 68, 42], [15, 54, 24, 5, 55, 25], [15, 43, 15, 10, 44, 16], [4, 144, 116, 4, 145, 117], [17, 68, 42], [17, 50, 22, 6, 51, 23], [19, 46, 16, 6, 47, 17], [2, 139, 111, 7, 140, 112], [17, 74, 46], [7, 54, 24, 16, 55, 25], [34, 37, 13], [4, 151, 121, 5, 152, 122], [4, 75, 47, 14, 76, 48], [11, 54, 24, 14, 55, 25], [16, 45, 15, 14, 46, 16], [6, 147, 117, 4, 148, 118], [6, 73, 45, 14, 74, 46], [11, 54, 24, 16, 55, 25], [30, 46, 16, 2, 47, 17], [8, 132, 106, 4, 133, 107], [8, 75, 47, 13, 76, 48], [7, 54, 24, 22, 55, 25], [22, 45, 15, 13, 46, 16], [10, 142, 114, 2, 143, 115], [19, 74, 46, 4, 75, 47], [28, 50, 22, 6, 51, 23], [33, 46, 16, 4, 47, 17], [8, 152, 122, 4, 153, 123], [22, 73, 45, 3, 74, 46], [8, 53, 23, 26, 54, 24], [12, 45, 15, 28, 46, 16], [3, 147, 117, 10, 148, 118], [3, 73, 45, 23, 74, 46], [4, 54, 24, 31, 55, 25], [11, 45, 15, 31, 46, 16], [7, 146, 116, 7, 147, 117], [21, 73, 45, 7, 74, 46], [1, 53, 23, 37, 54, 24], [19, 45, 15, 26, 46, 16], [5, 145, 115, 10, 146, 116], [19, 75, 47, 10, 76, 48], [15, 54, 24, 25, 55, 25], [23, 45, 15, 25, 46, 16], [13, 145, 115, 3, 146, 116], [2, 74, 46, 29, 75, 47], [42, 54, 24, 1, 55, 25], [23, 45, 15, 28, 46, 16], [17, 145, 115], [10, 74, 46, 23, 75, 47], [10, 54, 24, 35, 55, 25], [19, 45, 15, 35, 46, 16], [17, 145, 115, 1, 146, 116], [14, 74, 46, 21, 75, 47], [29, 54, 24, 19, 55, 25], [11, 45, 15, 46, 46, 16], [13, 145, 115, 6, 146, 116], [14, 74, 46, 23, 75, 47], [44, 54, 24, 7, 55, 25], [59, 46, 16, 1, 47, 17], [12, 151, 121, 7, 152, 122], [12, 75, 47, 26, 76, 48], [39, 54, 24, 14, 55, 25], [22, 45, 15, 41, 46, 16], [6, 151, 121, 14, 152, 122], [6, 75, 47, 34, 76, 48], [46, 54, 24, 10, 55, 25], [2, 45, 15, 64, 46, 16], [17, 152, 122, 4, 153, 123], [29, 74, 46, 14, 75, 47], [49, 54, 24, 10, 55, 25], [24, 45, 15, 46, 46, 16], [4, 152, 122, 18, 153, 123], [13, 74, 46, 32, 75, 47], [48, 54, 24, 14, 55, 25], [42, 45, 15, 32, 46, 16], [20, 147, 117, 4, 148, 118], [40, 75, 47, 7, 76, 48], [43, 54, 24, 22, 55, 25], [10, 45, 15, 67, 46, 16], [19, 148, 118, 6, 149, 119], [18, 75, 47, 31, 76, 48], [34, 54, 24, 34, 55, 25], [20, 45, 15, 61, 46, 16]], j.getRSBlocks = function (a, b) { var c = j.getRsBlockTable(a, b); if (void 0 == c) throw new Error("bad rs block @ typeNumber:" + a + "/errorCorrectLevel:" + b); for (var d = c.length / 3, e = [], f = 0; d > f; f++)for (var g = c[3 * f + 0], h = c[3 * f + 1], i = c[3 * f + 2], k = 0; g > k; k++)e.push(new j(h, i)); return e }, j.getRsBlockTable = function (a, b) { switch (b) { case d.L: return j.RS_BLOCK_TABLE[4 * (a - 1) + 0]; case d.M: return j.RS_BLOCK_TABLE[4 * (a - 1) + 1]; case d.Q: return j.RS_BLOCK_TABLE[4 * (a - 1) + 2]; case d.H: return j.RS_BLOCK_TABLE[4 * (a - 1) + 3]; default: return void 0 } }, k.prototype = { get: function (a) { var b = Math.floor(a / 8); return 1 == (1 & this.buffer[b] >>> 7 - a % 8) }, put: function (a, b) { for (var c = 0; b > c; c++)this.putBit(1 == (1 & a >>> b - c - 1)) }, getLengthInBits: function () { return this.length }, putBit: function (a) { var b = Math.floor(this.length / 8); this.buffer.length <= b && this.buffer.push(0), a && (this.buffer[b] |= 128 >>> this.length % 8), this.length++ } }; var l = [[17, 14, 11, 7], [32, 26, 20, 14], [53, 42, 32, 24], [78, 62, 46, 34], [106, 84, 60, 44], [134, 106, 74, 58], [154, 122, 86, 64], [192, 152, 108, 84], [230, 180, 130, 98], [271, 213, 151, 119], [321, 251, 177, 137], [367, 287, 203, 155], [425, 331, 241, 177], [458, 362, 258, 194], [520, 412, 292, 220], [586, 450, 322, 250], [644, 504, 364, 280], [718, 560, 394, 310], [792, 624, 442, 338], [858, 666, 482, 382], [929, 711, 509, 403], [1003, 779, 565, 439], [1091, 857, 611, 461], [1171, 911, 661, 511], [1273, 997, 715, 535], [1367, 1059, 751, 593], [1465, 1125, 805, 625], [1528, 1190, 868, 658], [1628, 1264, 908, 698], [1732, 1370, 982, 742], [1840, 1452, 1030, 790], [1952, 1538, 1112, 842], [2068, 1628, 1168, 898], [2188, 1722, 1228, 958], [2303, 1809, 1283, 983], [2431, 1911, 1351, 1051], [2563, 1989, 1423, 1093], [2699, 2099, 1499, 1139], [2809, 2213, 1579, 1219], [2953, 2331, 1663, 1273]], o = function () { var a = function (a, b) { this._el = a, this._htOption = b }; return a.prototype.draw = function (a) { function g(a, b) { var c = document.createElementNS("http://www.w3.org/2000/svg", a); for (var d in b) b.hasOwnProperty(d) && c.setAttribute(d, b[d]); return c } var b = this._htOption, c = this._el, d = a.getModuleCount(); Math.floor(b.width / d), Math.floor(b.height / d), this.clear(); var h = g("svg", { viewBox: "0 0 " + String(d) + " " + String(d), width: "100%", height: "100%", fill: b.colorLight }); h.setAttributeNS("http://www.w3.org/2000/xmlns/", "xmlns:xlink", "http://www.w3.org/1999/xlink"), c.appendChild(h), h.appendChild(g("rect", { fill: b.colorDark, width: "1", height: "1", id: "template" })); for (var i = 0; d > i; i++)for (var j = 0; d > j; j++)if (a.isDark(i, j)) { var k = g("use", { x: String(i), y: String(j) }); k.setAttributeNS("http://www.w3.org/1999/xlink", "href", "#template"), h.appendChild(k) } }, a.prototype.clear = function () { for (; this._el.hasChildNodes();)this._el.removeChild(this._el.lastChild) }, a }(), p = "svg" === document.documentElement.tagName.toLowerCase(), q = p ? o : m() ? function () { function a() { this._elImage.src = this._elCanvas.toDataURL("image/png"), this._elImage.style.display = "block", this._elCanvas.style.display = "none" } function d(a, b) { var c = this; if (c._fFail = b, c._fSuccess = a, null === c._bSupportDataURI) { var d = document.createElement("img"), e = function () { c._bSupportDataURI = !1, c._fFail && _fFail.call(c) }, f = function () { c._bSupportDataURI = !0, c._fSuccess && c._fSuccess.call(c) }; return d.onabort = e, d.onerror = e, d.onload = f, d.src = "data:image/gif;base64,iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==", void 0 } c._bSupportDataURI === !0 && c._fSuccess ? c._fSuccess.call(c) : c._bSupportDataURI === !1 && c._fFail && c._fFail.call(c) } if (this._android && this._android <= 2.1) { var b = 1 / window.devicePixelRatio, c = CanvasRenderingContext2D.prototype.drawImage; CanvasRenderingContext2D.prototype.drawImage = function (a, d, e, f, g, h, i, j) { if ("nodeName" in a && /img/i.test(a.nodeName)) for (var l = arguments.length - 1; l >= 1; l--)arguments[l] = arguments[l] * b; else "undefined" == typeof j && (arguments[1] *= b, arguments[2] *= b, arguments[3] *= b, arguments[4] *= b); c.apply(this, arguments) } } var e = function (a, b) { this._bIsPainted = !1, this._android = n(), this._htOption = b, this._elCanvas = document.createElement("canvas"), this._elCanvas.width = b.width, this._elCanvas.height = b.height, a.appendChild(this._elCanvas), this._el = a, this._oContext = this._elCanvas.getContext("2d"), this._bIsPainted = !1, this._elImage = document.createElement("img"), this._elImage.style.display = "none", this._el.appendChild(this._elImage), this._bSupportDataURI = null }; return e.prototype.draw = function (a) { var b = this._elImage, c = this._oContext, d = this._htOption, e = a.getModuleCount(), f = d.width / e, g = d.height / e, h = Math.round(f), i = Math.round(g); b.style.display = "none", this.clear(); for (var j = 0; e > j; j++)for (var k = 0; e > k; k++) { var l = a.isDark(j, k), m = k * f, n = j * g; c.strokeStyle = l ? d.colorDark : d.colorLight, c.lineWidth = 1, c.fillStyle = l ? d.colorDark : d.colorLight, c.fillRect(m, n, f, g), c.strokeRect(Math.floor(m) + .5, Math.floor(n) + .5, h, i), c.strokeRect(Math.ceil(m) - .5, Math.ceil(n) - .5, h, i) } this._bIsPainted = !0 }, e.prototype.makeImage = function () { this._bIsPainted && d.call(this, a) }, e.prototype.isPainted = function () { return this._bIsPainted }, e.prototype.clear = function () { this._oContext.clearRect(0, 0, this._elCanvas.width, this._elCanvas.height), this._bIsPainted = !1 }, e.prototype.round = function (a) { return a ? Math.floor(1e3 * a) / 1e3 : a }, e }() : function () { var a = function (a, b) { this._el = a, this._htOption = b }; return a.prototype.draw = function (a) { for (var b = this._htOption, c = this._el, d = a.getModuleCount(), e = Math.floor(b.width / d), f = Math.floor(b.height / d), g = ['<table style="border:0;border-collapse:collapse;">'], h = 0; d > h; h++) { g.push("<tr>"); for (var i = 0; d > i; i++)g.push('<td style="border:0;border-collapse:collapse;padding:0;margin:0;width:' + e + "px;height:" + f + "px;background-color:" + (a.isDark(h, i) ? b.colorDark : b.colorLight) + ';"></td>'); g.push("</tr>") } g.push("</table>"), c.innerHTML = g.join(""); var j = c.childNodes[0], k = (b.width - j.offsetWidth) / 2, l = (b.height - j.offsetHeight) / 2; k > 0 && l > 0 && (j.style.margin = l + "px " + k + "px") }, a.prototype.clear = function () { this._el.innerHTML = "" }, a }(); QRCode = function (a, b) {
                    if (this._htOption = { width: 256, , typeNumber: 4, colorDark: "#000000", colorLight: "#ffffff", correctLevel: d.H }, "string" == typeof b && (b = { text: b }), b) for (var c in b)
                        this._htOption[c] = b[c]; "string" == typeof a && (a = document.getElementById(a)),
                            this._android = n(), this._el = a,
                            this._oQRCode = null, this._oDrawing = new q(this._el, this._htOption), this._htOption.text && this.makeCode(this._htOption.text)
                },
                    QRCode.prototype.makeCode = function (a) {
                        this._oQRCode = new b(r(a, this._htOption.correctLevel), this._htOption.correctLevel),
                        this._oQRCode.addData(a), this._oQRCode.make(), this._el.title = a, this._oDrawing.draw(this._oQRCode), this.makeImage()
                    },
                    QRCode.prototype.makeImage = function () { "function" == typeof this._oDrawing.makeImage && (!this._android || this._android >= 3) && this._oDrawing.makeImage() },
                    QRCode.prototype.clear = function () { this._oDrawing.clear() }, QRCode.CorrectLevel = d
            }();]]></script>
 </head>
 <body>
  <p><img src="data:image/jpg;base64,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" style="width: 800px; " /></p> 
  <table border="0" cellpadding="0" cellspacing="0" style="border-color: blue; width: 800px;" width="800"> 
   <tbody> 
    <tr valign="top"> 
     <td style="width: 350px;"> 
      <table align="left" border="0" cellpadding="0" cellspacing="0" style="font-size: 9px; border-top-color: #000000; border-bottom-color: #000000; border-top-width: 3px; border-bottom-width: 3px; border-top-style: solid; border-bottom-style: solid;" width="350"> 
       <tbody> 
        <tr align="left"> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="//n1:DespatchAdvice"> 
              <xsl:for-each select="cac:DespatchSupplierParty "> 
               <xsl:for-each select="cac:Party">  
                <xsl:if test="cac:PartyName"> 
                 <xsl:value-of select="cac:PartyName/cbc:Name" /> 
                 <br /> 
                </xsl:if> 
                <xsl:for-each select="cac:Person"> 
                 <xsl:for-each select="cbc:Title"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                  <span> 
                   <xsl:text> 
                   </xsl:text> </span> 
                 </xsl:for-each> 
                 <xsl:for-each select="cbc:FirstName"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                  <span> 
                   <xsl:text> 
                   </xsl:text> </span> 
                 </xsl:for-each> 
                 <xsl:for-each select="cbc:MiddleName"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                  <span> 
                   <xsl:text> 
                   </xsl:text> </span> 
                 </xsl:for-each> 
                 <xsl:for-each select="cbc:FamilyName"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                  <span> 
                   <xsl:text> 
                   </xsl:text> </span> 
                 </xsl:for-each> 
                 <xsl:for-each select="cbc:NameSuffix"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                 </xsl:for-each> 
                </xsl:for-each>  
               </xsl:for-each> 
              </xsl:for-each> 
             </xsl:for-each>  </span> </span> </span></td> 
        </tr> 
        <tr align="left"> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="//n1:DespatchAdvice"> 
              <xsl:for-each select="cac:DespatchSupplierParty "> 
               <xsl:for-each select="cac:Party"> 
                <xsl:for-each select="cac:PostalAddress"> 
                 <xsl:for-each select="cbc:StreetName"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                  <span> 
                   <xsl:text> 
                   </xsl:text> </span> 
                 </xsl:for-each> 
                 <xsl:for-each select="cbc:BuildingName"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                 </xsl:for-each> 
                 <xsl:if test="cbc:BuildingNumber"> 
                  <span> 
                   <xsl:text>
                     No:
                   </xsl:text> </span> 
                  <xsl:for-each select="cbc:BuildingNumber"> 
                   <xsl:apply-templates></xsl:apply-templates> 
                  </xsl:for-each> 
                  <span> 
                   <xsl:text> 
                   </xsl:text> </span> 
                 </xsl:if> 
                 <br /> 
                 <xsl:for-each select="cbc:PostalZone"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                  <span> 
                   <xsl:text> 
                   </xsl:text> </span> 
                 </xsl:for-each> 
                 <xsl:for-each select="cbc:CitySubdivisionName"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                 </xsl:for-each> 
                 <span> 
                  <xsl:text>
                   / 
                  </xsl:text> </span> 
                 <xsl:for-each select="cbc:CityName"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                  <span> 
                   <xsl:text> 
                   </xsl:text> </span> 
                 </xsl:for-each> 
                </xsl:for-each> 
               </xsl:for-each> 
              </xsl:for-each> 
             </xsl:for-each>  </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 9px;">&#350;ube No: MERS&#304;N &#350;B. KARACALYAS EVREN MAH. 100.YIL CAD. A BLOK NO.2 &#304;&#199; KAPI NO:10 AKDEN&#304;Z/MERS&#304;N</span></span></td> 
        </tr> 
        <tr align="left"> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:value-of select="n1:DespatchAdvice/cac:DespatchSupplierParty/cac:Party/cac:PostalAddress/cac:Country/cbc:Name"> 
             </xsl:value-of>  </span> </span> </span></td> 
        </tr> 
        <tr align="left"> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">Tel:&#160;
             <xsl:value-of select="n1:DespatchAdvice/cac:DespatchSupplierParty/cac:Party/cac:Contact/cbc:Telephone"></xsl:value-of> Faks: 
             <xsl:value-of select="n1:DespatchAdvice/cac:DespatchSupplierParty/cac:Party/cac:Contact/cbc:Telefax"></xsl:value-of> </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">Web Sitesi:&#160;
             <xsl:value-of select="n1:DespatchAdvice/cac:DespatchSupplierParty/cac:Party/cbc:WebsiteURI"></xsl:value-of> </span> </span> </span></td> 
        </tr> 
        <tr align="left"> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">E-posta: 
             <xsl:value-of select="n1:DespatchAdvice/cac:DespatchSupplierParty/cac:Party/cac:Contact/cbc:ElectronicMail"></xsl:value-of> </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">Vergi Dairesi: 
             <xsl:value-of select="n1:DespatchAdvice/cac:DespatchSupplierParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:Name"></xsl:value-of> </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="//n1:DespatchAdvice/cac:DespatchSupplierParty/cac:Party/cac:PartyIdentification"> 
              <xsl:variable name="schemeID" select="cbc:ID/@schemeID"></xsl:variable> 
              <xsl:if test="$schemeID ='TCKN' or $schemeID ='VKN'"> 
               <xsl:value-of select="$schemeID"></xsl:value-of> : 
               <xsl:value-of select="cbc:ID"></xsl:value-of> 
              </xsl:if> 
             </xsl:for-each>  </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">Mersis No: 
             <xsl:value-of select="n1:DespatchAdvice/cac:DespatchSupplierParty/cac:Party/cac:PartyIdentification/cbc:ID[@schemeID = 'MERSISNO']"></xsl:value-of> </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">Ticaret Sicil No: 
             <xsl:value-of select="n1:DespatchAdvice/cac:DespatchSupplierParty/cac:Party/cac:PartyIdentification/cbc:ID[@schemeID = 'TICARETSICILNO']"></xsl:value-of> </span> </span> </span></td> 
        </tr> 
       </tbody> 
      </table> </td> 
     <td style="width: 147.25px; text-align: center;"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"><img align="middle" alt="GIB Logo" src="data:image/jpeg;base64,/9j/4QAYRXhpZgAASUkqAAgAAAAAAAAAAAAAAP/sABFEdWNreQABAAQAAABkAAD/4QMZaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wLwA8P3hwYWNrZXQgYmVnaW49Iu+7vyIgaWQ9Ilc1TTBNcENlaGlIenJlU3pOVGN6a2M5ZCI/PiA8eDp4bXBtZXRhIHhtbG5zOng9ImFkb2JlOm5zOm1ldGEvIiB4OnhtcHRrPSJBZG9iZSBYTVAgQ29yZSA1LjYtYzEzMiA3OS4xNTkyODQsIDIwMTYvMDQvMTktMTM6MTM6NDAgICAgICAgICI+IDxyZGY6UkRGIHhtbG5zOnJkZj0iaHR0cDovL3d3dy53My5vcmcvMTk5OS8wMi8yMi1yZGYtc3ludGF4LW5zIyI+IDxyZGY6RGVzY3JpcHRpb24gcmRmOmFib3V0PSIiIHhtbG5zOnhtcE1NPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvbW0vIiB4bWxuczpzdFJlZj0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL3NUeXBlL1Jlc291cmNlUmVmIyIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bXBNTTpEb2N1bWVudElEPSJ4bXAuZGlkOjZDNDJBNEI2QjVCRDExRThCQjM0REIwQkZGMEQxODY0IiB4bXBNTTpJbnN0YW5jZUlEPSJ4bXAuaWlkOjZDNDJBNEI1QjVCRDExRThCQjM0REIwQkZGMEQxODY0IiB4bXA6Q3JlYXRvclRvb2w9IkFkb2JlIFBob3Rvc2hvcCBDUzQgV2luZG93cyI+IDx4bXBNTTpEZXJpdmVkRnJvbSBzdFJlZjppbnN0YW5jZUlEPSIzREVENkU1N0FDREVDNEJBNzkxNUM2M0NCN0RENzM0NyIgc3RSZWY6ZG9jdW1lbnRJRD0iM0RFRDZFNTdBQ0RFQzRCQTc5MTVDNjNDQjdERDczNDciLz4gPC9yZGY6RGVzY3JpcHRpb24+IDwvcmRmOlJERj4gPC94OnhtcG1ldGE+IDw/eHBhY2tldCBlbmQ9InIiPz7/7gAOQWRvYmUAZMAAAAAB/9sAhAABAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB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" style="width: 70px;" /><br /><br /><span align="center">e-&#304;RSAL&#304;YE</span> </span> </span> </span></td> 
     <td align="right" style="width: 242px;"> <p></p> 
      <div id="qrcode">&#160;
      </div> 
      <div id="qrcode"></div>
      <div id="qrvalue" style="visibility: hidden; height: 30px; width: 30px; display: none;">
       {"vkntckn": "<xsl:value-of select="n1:DespatchAdvice/cac:DespatchSupplierParty/cac:Party/cac:PartyIdentification/cbc:ID[@schemeID = 'TCKN' or @schemeID = 'VKN']"/>","avkntckn": "<xsl:value-of select="n1:DespatchAdvice/cac:DeliveryCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID[@schemeID = 'TCKN' or @schemeID = 'VKN']"/>","senaryo": "<xsl:value-of select="n1:DespatchAdvice/cbc:ProfileID"/>","tip": "<xsl:value-of select="n1:DespatchAdvice/cbc:DespatchAdviceTypeCode"/>","tarih": "<xsl:value-of select="n1:DespatchAdvice/cbc:IssueDate"/>","no": "<xsl:value-of select="n1:DespatchAdvice/cbc:ID"/>","ettn": "<xsl:value-of select="n1:DespatchAdvice/cbc:UUID"/>","sevktarihi": "<xsl:value-of select="n1:DespatchAdvice/cac:Shipment/cac:Delivery/cac:Despatch/cbc:ActualDespatchDate"/>","sevkzamani": "<xsl:value-of select="n1:DespatchAdvice/cac:Shipment/cac:Delivery/cac:Despatch/cbc:ActualDespatchTime"/>","tasiyicivkn": "<xsl:value-of select="n1:DespatchAdvice/cac:Shipment/cac:Delivery/cac:CarrierParty/cac:PartyIdentification/cbc:ID"/>","plaka": "<xsl:value-of select="n1:DespatchAdvice/cac:Shipment/cac:ShipmentStage/cac:TransportMeans/cac:RoadTransport/cbc:LicensePlateID"/>"}
      </div><script type="text/javascript">var qrcode = new QRCode(document.getElementById("qrcode"), { width: 140, height: 140, correctLevel: QRCode.CorrectLevel.L }); qrcode.makeCode(document.getElementById("qrvalue").innerHTML)</script></td> 
    </tr> 
    <tr > 
     <td style="width: 350px;"> 
      <table align="left" border="0" cellpadding="0" cellspacing="0" id="customerPartyTable" style="border-top-color: #000000; border-bottom-color: #000000; border-top-width: 3px; border-bottom-width: 3px; border-top-style: solid; border-bottom-style: solid;" width="350"> 
       <tbody> 
        <tr> 
         <td align="left"><span style="font-family: verdana,geneva,sans-serif; font-weight: bold;"><span style="color: #696969;"><span style="font-size: 10px;">SAYIN</span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="//n1:DespatchAdvice"> 
              <xsl:for-each select="cac:DeliveryCustomerParty"> 
               <xsl:for-each select="cac:Party">  
                <xsl:if test="cac:PartyName"> 
                 <xsl:value-of select="cac:PartyName/cbc:Name" /> 
                 <br /> 
                </xsl:if> 
                <xsl:for-each select="cac:Person"> 
                 <xsl:for-each select="cbc:Title"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                  <span> 
                   <xsl:text> 
                   </xsl:text> </span> 
                 </xsl:for-each> 
                 <xsl:for-each select="cbc:FirstName"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                  <span> 
                   <xsl:text> 
                   </xsl:text> </span> 
                 </xsl:for-each> 
                 <xsl:for-each select="cbc:MiddleName"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                  <span> 
                   <xsl:text> 
                   </xsl:text> </span> 
                 </xsl:for-each> 
                 <xsl:for-each select="cbc:FamilyName"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                  <span> 
                   <xsl:text> 
                   </xsl:text> </span> 
                 </xsl:for-each> 
                 <xsl:for-each select="cbc:NameSuffix"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                 </xsl:for-each> 
                </xsl:for-each>  
               </xsl:for-each> 
              </xsl:for-each> 
             </xsl:for-each>  </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="//n1:DespatchAdvice"> 
              <xsl:for-each select=" cac:DeliveryCustomerParty"> 
               <xsl:for-each select="cac:Party"> 
                <xsl:for-each select="cac:PostalAddress"> 
                 <xsl:for-each select="cbc:StreetName"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                  <span> 
                   <xsl:text> 
                   </xsl:text> </span> 
                 </xsl:for-each> 
                 <xsl:for-each select="cbc:BuildingName"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                 </xsl:for-each> 
                 <xsl:if test="cbc:BuildingNumber"> 
                  <span> 
                   <xsl:text>
                     No:
                   </xsl:text> </span> 
                  <xsl:for-each select="cbc:BuildingNumber"> 
                   <xsl:apply-templates></xsl:apply-templates> 
                  </xsl:for-each> 
                  <span> 
                   <xsl:text> 
                   </xsl:text> </span> 
                 </xsl:if> 
                 <br /> 
                 <xsl:for-each select="cbc:PostalZone"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                  <span> 
                   <xsl:text> 
                   </xsl:text> </span> 
                 </xsl:for-each> 
                 <xsl:for-each select="cbc:CitySubdivisionName"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                 </xsl:for-each> 
                 <span> 
                  <xsl:text>
                   / 
                  </xsl:text> </span> 
                 <xsl:for-each select="cbc:CityName"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                  <span> 
                   <xsl:text> 
                   </xsl:text> </span> 
                 </xsl:for-each> 
                </xsl:for-each> 
               </xsl:for-each> 
              </xsl:for-each> 
             </xsl:for-each>  </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:value-of select="n1:DespatchAdvice/cac:DeliveryCustomerParty/cac:Party/cac:PostalAddress/cac:Country/cbc:Name"> 
             </xsl:value-of>  </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"><strong>Teslimat Adresi</strong>:&#160;
             <xsl:value-of select="n1:DespatchAdvice/cac:Shipment/cac:Delivery/cac:DeliveryAddress/cbc:StreetName"></xsl:value-of> </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"> 
             <xsl:value-of select="n1:DespatchAdvice/cac:Shipment/cac:Delivery/cac:DeliveryAddress/cbc:CitySubdivisionName"></xsl:value-of> 
             <xsl:text>
              /
             </xsl:text> 
             <xsl:value-of select="n1:DespatchAdvice/cac:Shipment/cac:Delivery/cac:DeliveryAddress/cbc:CityName"></xsl:value-of> </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:value-of select="n1:DespatchAdvice/cac:Shipment/cac:Delivery/cac:DeliveryAddress/cac:Country/cbc:Name"></xsl:value-of> </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">Tel:&#160;
             <xsl:value-of select="n1:DespatchAdvice/cac:DeliveryCustomerParty/cac:Party/cac:Contact/cbc:Telephone"></xsl:value-of>&#160;Faks:&#160;
             <xsl:value-of select="n1:DespatchAdvice/cac:DeliveryCustomerParty/cac:Party/cac:Contact/cbc:Telefax"></xsl:value-of> </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">Web Sitesi:&#160;
             <xsl:value-of select="n1:DespatchAdvice/cac:DeliveryCustomerParty/cac:Party/cbc:WebsiteURI"></xsl:value-of> </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">E-Posta:&#160;
             <xsl:value-of select="n1:DespatchAdvice/cac:DeliveryCustomerParty/cac:Party/cac:Contact/cbc:ElectronicMail"></xsl:value-of> </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">Vergi Dairesi:&#160;
             <xsl:value-of select="n1:DespatchAdvice/cac:DeliveryCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:Name"></xsl:value-of> </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="//n1:DespatchAdvice/cac:DeliveryCustomerParty/cac:Party/cac:PartyIdentification"> 
              <xsl:variable name="schemeID" select="cbc:ID/@schemeID"></xsl:variable> 
              <xsl:if test="$schemeID ='TCKN' or $schemeID ='VKN'"> 
               <xsl:value-of select="$schemeID"></xsl:value-of> : 
               <xsl:value-of select="cbc:ID"></xsl:value-of> 
              </xsl:if> 
             </xsl:for-each>  </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">Mersis No:&#160;
             <xsl:value-of select="n1:DespatchAdvice/cac:DeliveryCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID[@schemeID = 'MERSISNO']"></xsl:value-of> </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">Ticaret No:&#160;
             <xsl:value-of select="n1:DespatchAdvice/cac:DeliveryCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID[@schemeID = 'TICARETSICILNO']"></xsl:value-of> </span> </span> </span></td> 
        </tr> 
       </tbody> 
      </table> </td> 
     <td style="width: 147.25px;">&#160;</td> 
     <td align="right" style="width: 299.194px;"> 
      <table style="width: 242px; border-collapse: collapse;"> 
       <tbody> 
        <tr> 
         <td style="border: 0.05em solid rgb(132, 132, 132);"><span style="font-family: verdana,geneva,sans-serif; font-weight: bold;"><span style="color: #696969;"><span style="font-size: 10px;">&#214;zelle&#351;tirme No:</span> </span> </span></td> 
         <td style="border: 0.05em solid rgb(132, 132, 132); width: 129px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="n1:DespatchAdvice/cbc:CustomizationID"> 
              <xsl:apply-templates />
             </xsl:for-each>  </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td style="border: 0.05em solid rgb(132, 132, 132);"><span style="font-family: verdana,geneva,sans-serif; font-weight: bold;"><span style="color: #696969;"><span style="font-size: 10px;">Senaryo:</span> </span> </span></td> 
         <td style="border: 0.05em solid rgb(132, 132, 132); width: 129px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="n1:DespatchAdvice/cbc:ProfileID"> 
              <xsl:apply-templates></xsl:apply-templates>
             </xsl:for-each>  </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td style="border: 0.05em solid rgb(132, 132, 132);"><span style="font-family: verdana,geneva,sans-serif; font-weight: bold;"><span style="color: #696969;"><span style="font-size: 10px;">&#304;rsaliye Tipi:</span> </span> </span></td> 
         <td style="border: 0.05em solid rgb(132, 132, 132); width: 129px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="n1:DespatchAdvice/cbc:DespatchAdviceTypeCode"> 
              <xsl:apply-templates /> 
             </xsl:for-each>  </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td style="border: 0.05em solid rgb(132, 132, 132);"><span style="font-family: verdana,geneva,sans-serif; font-weight: bold;"><span style="color: #696969;"><span style="font-size: 10px;">&#304;rsaliye No:</span> </span> </span></td> 
         <td style="border: 0.05em solid rgb(132, 132, 132); width: 129px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="n1:DespatchAdvice/cbc:ID"> 
              <xsl:apply-templates />
             </xsl:for-each>  </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td style="border: 0.05em solid rgb(132, 132, 132);"><span style="font-family: verdana,geneva,sans-serif; font-weight: bold;"><span style="color: #696969;"><span style="font-size: 10px;">&#304;rsaliye Tarihi:</span> </span> </span></td> 
         <td style="border: 0.05em solid rgb(132, 132, 132); width: 129px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="n1:DespatchAdvice/cbc:IssueDate"> 
              <xsl:apply-templates select="." />
             </xsl:for-each>  </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td style="border: 0.05em solid rgb(132, 132, 132);"><span style="font-family: verdana,geneva,sans-serif; font-weight: bold;"><span style="color: #696969;"><span style="font-size: 10px;">&#304;rsaliye Zaman&#305;:</span> </span> </span></td> 
         <td style="border: 0.05em solid rgb(132, 132, 132); width: 129px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="n1:DespatchAdvice/cbc:IssueTime"> 
              <xsl:apply-templates select="."></xsl:apply-templates>
             </xsl:for-each>  </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td style="border: 0.05em solid rgb(132, 132, 132);"><span style="font-family: verdana,geneva,sans-serif; font-weight: bold;"><span style="color: #696969;"><span style="font-size: 10px;">SevkTarihi:</span> </span> </span></td> 
         <td style="border: 0.05em solid rgb(132, 132, 132); width: 129px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="n1:DespatchAdvice/cac:Shipment/cac:Delivery/cac:Despatch/cbc:ActualDespatchDate"> 
              <xsl:apply-templates select="."></xsl:apply-templates>
             </xsl:for-each>  </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td style="border: 0.05em solid rgb(132, 132, 132);"><span style="font-family: verdana,geneva,sans-serif; font-weight: bold;"><span style="color: #696969;"><span style="font-size: 10px;">Sevk Zaman&#305;:</span> </span> </span></td> 
         <td style="border: 0.05em solid rgb(132, 132, 132); width: 129px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="n1:DespatchAdvice/cac:Shipment/cac:Delivery/cac:Despatch/cbc:ActualDespatchTime"> 
              <xsl:apply-templates select="." />
             </xsl:for-each>  </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td style="border: 0.05em solid rgb(132, 132, 132);"><span style="font-family: verdana,geneva,sans-serif; font-weight: bold;"><span style="color: #696969;"><span style="font-size: 10px;">Sipari&#351; No:</span> </span> </span></td> 
         <td style="border: 0.05em solid rgb(132, 132, 132); width: 129px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="n1:DespatchAdvice/cac:OrderReference/cbc:ID"> 
              <xsl:apply-templates />
             </xsl:for-each>  </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td style="border: 0.05em solid rgb(132, 132, 132);"><span style="font-family: verdana,geneva,sans-serif; font-weight: bold;"><span style="color: #696969;"><span style="font-size: 10px;">Sipari&#351; Tarihi:</span> </span> </span></td> 
         <td style="border: 0.05em solid rgb(132, 132, 132); width: 129px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="n1:DespatchAdvice/cac:OrderReference/cbc:IssueDate"> 
              <xsl:apply-templates select="." />
             </xsl:for-each>  </span> </span> </span></td> 
        </tr> 
       </tbody> 
      </table> </td> 
    </tr> 
    <tr > 
     <td style="width: 350px;"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif; font-weight: bold;">ETTN: </span> 
        <xsl:for-each select="n1:DespatchAdvice/cbc:UUID"> 
         <xsl:apply-templates /> 
        </xsl:for-each>  </span> </span></td> 
     <td style="width: 147.25px;">&#160;</td> 
     <td style="width: 299.194px;">&#160;</td> 
    </tr> 
   </tbody> 
  </table>
 </body>
</html><html>
 <head></head>
 <body>
   
  <table style="border: 2px solid rgb(0, 0, 0); width: 800px; border-collapse: collapse;"> 
   <tbody> 
    <tr> 
     <td style="border: 0.05em solid rgb(132, 132, 132); width: 3%; text-align: center; background-color: #000000;"><span style="color: #ffffff;"><span style="font-size: 9px;"><strong><span style="font-family: verdana,geneva,sans-serif;">S&#305;ra No</span></strong></span></span></td> 
     <td style="border: 0.05em solid rgb(132, 132, 132); width: 3%; text-align: center; background-color: #000000;"><span style="color: #ffffff;"><span style="font-size: 9px;"><strong><span style="font-family: verdana,geneva,sans-serif;">Malzeme Kodu</span></strong></span></span></td> 
     <td style="border: 0.05em solid rgb(132, 132, 132); width: 3%; text-align: center; background-color: #000000;"><span style="color: #ffffff;"><span style="font-size: 9px;"><strong><span style="font-family: verdana,geneva,sans-serif;">Malzeme A&#231;&#305;klamas&#305;</span></strong></span></span></td> 
     <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cbc:Note">
      <td condition="//n1:DespatchAdvice/cac:DespatchLine/cbc:Note" style="border: 0.05em solid rgb(132, 132, 132); width: 3%; text-align: center; background-color: #000000;"><span style="color: #ffffff;"><span style="font-size: 9px;"><strong><span style="font-family: verdana,geneva,sans-serif;">Sat&#305;r A&#231;&#305;klamas&#305;</span></strong></span></span></td>
     </xsl:if> 
     <td style="border: 0.05em solid rgb(132, 132, 132); width: 3%; text-align: center; background-color: #000000;"><span style="color: #ffffff;"><span style="font-size: 9px;"><strong><span style="font-family: verdana,geneva,sans-serif;">Miktar</span></strong></span></span></td> 
     <td style="border: 0.05em solid rgb(132, 132, 132); width: 3%; text-align: center; background-color: #000000;"><span style="color: #ffffff;"><span style="font-size: 9px;"><strong><span style="font-family: verdana,geneva,sans-serif;">Birim Fiyat</span></strong></span></span></td> 
     <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:AllowanceCharge">
      <td condition="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:AllowanceCharge" style="border: 0.05em solid rgb(132, 132, 132); width: 3%; text-align: center; background-color: #000000;"><span style="color: #ffffff;"><span style="font-size: 9px;"><strong><span style="font-family: verdana,geneva,sans-serif;">&#304;skonto Oran&#305;</span></strong></span></span></td>
     </xsl:if> 
     <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:AllowanceCharge">
      <td condition="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:AllowanceCharge" style="border: 0.05em solid rgb(132, 132, 132); width: 3%; text-align: center; background-color: #000000;"><span style="color: #ffffff;"><span style="font-size: 9px;"><strong><span style="font-family: verdana,geneva,sans-serif;">&#304;skonto Tutar&#305;</span></strong></span></span></td>
     </xsl:if> 
     <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme">
      <td condition="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme" style="border: 0.05em solid rgb(132, 132, 132); width: 3%; text-align: center; background-color: #000000;"><span style="color: #ffffff;"><span style="font-size: 9px;"><strong><span style="font-family: verdana,geneva,sans-serif;">KDV Oran&#305;</span></strong></span></span></td>
     </xsl:if> 
     <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme">
      <td condition="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme" style="border: 0.05em solid rgb(132, 132, 132); width: 3%; text-align: center; background-color: #000000;"><span style="color: #ffffff;"><span style="font-size: 9px;"><strong><span style="font-family: verdana,geneva,sans-serif;">KDV Tutar&#305;</span></strong></span></span></td>
     </xsl:if> 
     <xsl:if test="(//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal and //n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme and //n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode!=&apos;0015&apos;) or (//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:WithholdingTaxTotal/cac:TaxSubtotal)">
      <td condition="(//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal and //n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme and //n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode!=&apos;0015&apos;) or (//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:WithholdingTaxTotal/cac:TaxSubtotal)" style="border: 0.05em solid rgb(132, 132, 132); width: 3%; text-align: center; background-color: #000000;"><span style="color: #ffffff;"><span style="font-size: 9px;"><strong><span style="font-family: verdana,geneva,sans-serif;">Di&#287;er Vergiler</span></strong></span></span></td>
     </xsl:if> 
     <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:TaxExemptionReason">
      <td condition="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:TaxExemptionReason" style="border: 0.05em solid rgb(132, 132, 132); width: 3%; text-align: center; background-color: #000000;"><span style="color: #ffffff;"><span style="font-size: 9px;"><strong><span style="font-family: verdana,geneva,sans-serif;">Vergi Muafiyet Sebebi</span></strong></span></span></td>
     </xsl:if> 
     <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cbc:OutstandingQuantity">
      <td condition="//n1:DespatchAdvice/cac:DespatchLine/cbc:OutstandingQuantity" style="border: 0.05em solid rgb(132, 132, 132); width: 3%; text-align: center; background-color: #000000;"><span style="color: #ffffff;"><span style="font-size: 9px;"><strong><span style="font-family: verdana,geneva,sans-serif;">Sonra G&#246;nderilecek Miktar</span></strong></span></span></td>
     </xsl:if> 
     <td style="border: 0.05em solid rgb(132, 132, 132); width: 3%; text-align: center; background-color: #000000;"><span style="color: #ffffff;"><span style="font-size: 9px;"><strong><span style="font-family: verdana,geneva,sans-serif;">Tutar</span></strong></span></span></td> 
     <xsl:if test="//n1:DespatchAdvice/cbc:ProfileID =&apos;HKSIRSALIYE&apos;">
      <td condition="//n1:DespatchAdvice/cbc:ProfileID =&apos;HKSIRSALIYE&apos;" style="border: 0.05em solid rgb(132, 132, 132); width: 3%; text-align: center; background-color: #000000;"><span style="font-size: 9px;"><strong><span style="color: #ffffff;"><span style="font-family: verdana,geneva,sans-serif;">K&#252;nye Numaras</span></span><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;">&#305;</span></span></strong></span></td>
     </xsl:if> 
    </tr> 
    <xsl:for-each select="//n1:DespatchAdvice/cac:DespatchLine">
     <tr> 
      <td style="border: 0.05em solid rgb(132, 132, 132); width: 3%; text-align: left;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
          <xsl:value-of select="./cbc:ID"></xsl:value-of></span></span></span></td> 
      <td style="border: 0.05em solid rgb(132, 132, 132); width: 10%; text-align: center;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
          <xsl:value-of select="./cac:Item/cbc:Name"></xsl:value-of></span></span></span></td> 
      <td style="border: 0.05em solid rgb(132, 132, 132); width: 6%; text-align: center;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
          <xsl:value-of select="./cac:Item/cbc:Description "></xsl:value-of></span></span></span></td> 
      <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cbc:Note">
       <td condition="//n1:DespatchAdvice/cac:DespatchLine/cbc:Note" style="border: 0.05em solid rgb(132, 132, 132); width: 6%; text-align: center;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
           <xsl:value-of select="./cbc:Note" /></span></span></span></td>
      </xsl:if> 
      <td style="border: 0.05em solid rgb(132, 132, 132); width: 6%; text-align: right;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
          <xsl:value-of select="format-number(./cbc:DeliveredQuantity, &apos;###.##0,##&apos;, &apos;european&apos;)"></xsl:value-of> 
          <xsl:if test="./cbc:DeliveredQuantity/@unitCode"> 
           <xsl:for-each select="./cbc:DeliveredQuantity"> 
            <xsl:text></xsl:text> 
            <xsl:choose> 
             <xsl:when test="@unitCode  = &apos;AWG&apos;"> 
              <span> 
               <xsl:text>
                amerikan kablo &#246;l&#231;er
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;AYR&apos;"> 
              <span> 
               <xsl:text>
                alt&#305;n ayar&#305;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;BAS&apos;"> 
              <span> 
               <xsl:text>
                BAS
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;BPM&apos;"> 
              <span> 
               <xsl:text>
                dakika ba&#351;&#305;na vuru&#351;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;CPR&apos;"> 
              <span> 
               <xsl:text>
                ADET-&#199;&#304;FT
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;DBM&apos;"> 
              <span> 
               <xsl:text>
                desibel-milivat
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;DBW&apos;"> 
              <span> 
               <xsl:text>
                desibel-vat
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;FNU&apos;"> 
              <span> 
               <xsl:text>
                Formazin nefelometrik birim
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;GMS&apos;"> 
              <span> 
               <xsl:text>
                G&#220;M&#220;&#350;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;HAD&apos;"> 
              <span> 
               <xsl:text>
                Adet G&#252;n&#220;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;HMO&apos;"> 
              <span> 
               <xsl:text>
                Adet Ay&#305;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;HWE&apos;"> 
              <span> 
               <xsl:text>
                Adet Haftas&#305;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;IUG&apos;"> 
              <span> 
               <xsl:text>
                uluslararas&#305; birim ba&#351;&#305;na gram
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KFO&apos;"> 
              <span> 
               <xsl:text>
                D&#304;FOSFOR PENTAOKS&#304;T K&#304;LOGRAMI
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KH6&apos;"> 
              <span> 
               <xsl:text>
                K&#304;LOGRAM-BA&#350;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KHO&apos;"> 
              <span> 
               <xsl:text>
                H&#304;DROJEN PEROKS&#304;T K&#304;LOGRAMI
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KNM&apos;"> 
              <span> 
               <xsl:text>
                kilonewton b&#246;l&#252; metrekare
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KOH&apos;"> 
              <span> 
               <xsl:text>
                K&#304;LOGRAM POTASYUM H&#304;DROKS&#304;T
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KPR&apos;"> 
              <span> 
               <xsl:text>
                K&#304;LOGRAM-&#199;&#304;FT
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KWN&apos;"> 
              <span> 
               <xsl:text>
                KARAT
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KWS&apos;"> 
              <span> 
               <xsl:text>
                OTV Maktu Vergi
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KWY&apos;"> 
              <span> 
               <xsl:text>
                OTV birim fiyat&#305;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MKD&apos;"> 
              <span> 
               <xsl:text>
                B&#304;N K&#304;LOWATT SAAT
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MKM&apos;"> 
              <span> 
               <xsl:text>
                Metrekare Ay&#305;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MKW&apos;"> 
              <span> 
               <xsl:text>
                Metrekare Haftas&#305;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MNJ&apos;"> 
              <span> 
               <xsl:text>
                Normalize Metrek&#252;p ba&#351;&#305;na Mega Joule
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MQD&apos;"> 
              <span> 
               <xsl:text>
                Metrek&#252;p G&#252;n
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MQM&apos;"> 
              <span> 
               <xsl:text>
                Metrek&#252;p Ay
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MQW&apos;"> 
              <span> 
               <xsl:text>
                Metrek&#252;p Hafta
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MRD&apos;"> 
              <span> 
               <xsl:text>
                Metre G&#252;n
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MRM&apos;"> 
              <span> 
               <xsl:text>
                Metre Ay
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MRW&apos;"> 
              <span> 
               <xsl:text>
                Metre Hafta
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MTZ&apos;"> 
              <span> 
               <xsl:text>
                millihertz
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;NCR&apos;"> 
              <span> 
               <xsl:text>
                KARAT
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;NTU&apos;"> 
              <span> 
               <xsl:text>
                Nefelometrik bulan&#305;kl&#305;k birimi
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;ODG&apos;"> 
              <span> 
               <xsl:text>
                ODS Grams
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;ODK&apos;"> 
              <span> 
               <xsl:text>
                ODS Kilograms
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;ODM&apos;"> 
              <span> 
               <xsl:text>
                ODS Milligrams
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;OMV&apos;"> 
              <span> 
               <xsl:text>
                OTV Maktu Vergi
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;OPM&apos;"> 
              <span> 
               <xsl:text>
                Dakikadaki sal&#305;n&#305;mlar
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;OTB&apos;"> 
              <span> 
               <xsl:text>
                OTV birim fiyat&#305;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;Q29&apos;"> 
              <span> 
               <xsl:text>
                hektogram ba&#351;&#305;na mikrogram
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;Q30&apos;"> 
              <span> 
               <xsl:text>
                kilojoule/gram
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;Q31&apos;"> 
              <span> 
               <xsl:text>
                femtolitre
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;Q32&apos;"> 
              <span> 
               <xsl:text>
                pikolitre
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;Q33&apos;"> 
              <span> 
               <xsl:text>
                nanolitre
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;Q34&apos;"> 
              <span> 
               <xsl:text>
                megavat/dakika
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;Q35&apos;"> 
              <span> 
               <xsl:text>
                metrekare/metrek&#252;p
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;Q36&apos;"> 
              <span> 
               <xsl:text>
                G&#252;nl&#252;k standart metrek&#252;p
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;Q38&apos;"> 
              <span> 
               <xsl:text>
                Saatte standart metrek&#252;p
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;Q40&apos;"> 
              <span> 
               <xsl:text>
                Saatte normalle&#351;tirilmi&#351; metrek&#252;p
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;Q41&apos;"> 
              <span> 
               <xsl:text>
                Joule/normalle&#351;tirilmi&#351; metrek&#252;p
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;Q42&apos;"> 
              <span> 
               <xsl:text>
                Joule/standart metrek&#252;p
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;TPI&apos;"> 
              <span> 
               <xsl:text>
                in&#231; ba&#351;&#305;na di&#351;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;TWH&apos;"> 
              <span> 
               <xsl:text>
                B&#304;N K&#304;LOWATT SAAT
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;Z9&apos;"> 
              <span> 
               <xsl:text>
                nanomol
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;SA&apos;"> 
              <xsl:text>
               &#199;UVAL
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;26&apos;"> 
              <xsl:text>
               ton
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;BX&apos;"> 
              <xsl:text>
               Kutu
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;LTR&apos;"> 
              <xsl:text>
               lt
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;C62&apos;"> 
              <xsl:text>
               Adet
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;NIU&apos;"> 
              <xsl:text>
               Adet
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KGM&apos;"> 
              <xsl:text>
               kg
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KJO&apos;"> 
              <xsl:text>
               kJ
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;GRM&apos;"> 
              <xsl:text>
               g
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MGM&apos;"> 
              <xsl:text>
               mg
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;NT&apos;"> 
              <xsl:text>
               Net Ton
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;GT&apos;"> 
              <xsl:text>
               Gross Ton
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MTR&apos;"> 
              <xsl:text>
               m
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MMT&apos;"> 
              <xsl:text>
               mm
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KTM&apos;"> 
              <xsl:text>
               km
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MLT&apos;"> 
              <xsl:text>
               ml
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MMQ&apos;"> 
              <xsl:text>
               mm3
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;CLT&apos;"> 
              <xsl:text>
               cl
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;CMK&apos;"> 
              <xsl:text>
               cm2
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;CMQ&apos;"> 
              <xsl:text>
               cm3
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;CMT&apos;"> 
              <xsl:text>
               cm
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MTK&apos;"> 
              <xsl:text>
               m2
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MTQ&apos;"> 
              <xsl:text>
               m3
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;DAY&apos;"> 
              <xsl:text>
                G&#252;n
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MON&apos;"> 
              <xsl:text>
                Ay
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;PA&apos;"> 
              <xsl:text>
                Paket
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KWH&apos;"> 
              <xsl:text>
                KWH
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;ANN&apos;"> 
              <xsl:text>
                Y&#305;l
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;HUR&apos;"> 
              <xsl:text>
                Saat
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;D61&apos;"> 
              <xsl:text>
                Dakika
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;D62&apos;"> 
              <xsl:text>
                Saniye
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;CCT&apos;"> 
              <xsl:text>
                Ton ba&#351;.ta&#351;&#305;ma kap.
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;D30&apos;"> 
              <xsl:text>
                Br&#252;t kalori
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;D40&apos;"> 
              <xsl:text>
                1000 lt
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;LPA&apos;"> 
              <xsl:text>
                saf alkol lt
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;B32&apos;"> 
              <xsl:text>
                kg.m2
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;NCL&apos;"> 
              <xsl:text>
                h&#252;cre adet
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;PR&apos;"> 
              <xsl:text>
                &#199;ift
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;R9&apos;"> 
              <xsl:text>
                1000 m3
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;SET&apos;"> 
              <xsl:text>
                Set
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;T3&apos;"> 
              <xsl:text>
                1000 adet
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;E07&apos;"> 
              <xsl:text>
                MWh/h
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;EA&apos;"> 
              <xsl:text>
               Adet
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;D64&apos;"> 
              <xsl:text>
               BLOK
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;TN&apos;"> 
              <xsl:text>
               Teneke
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;5C&apos;"> 
              <xsl:text>
               Galon
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;BG&apos;"> 
              <xsl:text>
               Torba
              </xsl:text> 
             </xsl:when> 
            </xsl:choose> 
           </xsl:for-each> 
          </xsl:if></span></span></span></td> 
      <td style="border: 0.05em solid rgb(132, 132, 132); width: 13%; text-align: right;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"> 
          <xsl:value-of select="format-number(./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:Price/cbc:PriceAmount, &apos;###.##0,########&apos;, &apos;european&apos;)" /> 
          <xsl:if test="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:Price/cbc:PriceAmount/@currencyID"> 
           <xsl:text> 
           </xsl:text> 
           <xsl:if test="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:Price/cbc:PriceAmount/@currencyID = &quot;TRY&quot; or ./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:Price/cbc:PriceAmount/@currencyID = &quot;TRY&quot;"> 
            <xsl:text>
             TL
            </xsl:text> 
           </xsl:if> 
           <xsl:if test="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:Price/cbc:PriceAmount/@currencyID != &quot;TRY&quot; and ./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:Price/cbc:PriceAmount/@currencyID != &quot;TRY&quot;"> 
            <xsl:value-of select="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:Price/cbc:PriceAmount/@currencyID" /> 
           </xsl:if> 
          </xsl:if></span></span></span></td> 
      <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:AllowanceCharge">
       <td condition="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:AllowanceCharge" style="border: 0.05em solid rgb(132, 132, 132); width: 13%; text-align: right;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
           <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:AllowanceCharge">  
            <xsl:for-each select="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:AllowanceCharge"> 
             <table border="0" width="100%" cellspacing="0px" cellpadding="0px" style="border-collapse:collapse;"> 
              <tbody>
               <tr> 
                <td align="right"> <span> 
                  <xsl:text>
                   %
                  </xsl:text> 
                  <xsl:value-of select="format-number(./cbc:MultiplierFactorNumeric*100, &apos;###.##0,00&apos;, &apos;european&apos;)" /> </span> </td> 
               </tr> 
              </tbody>
             </table> 
            </xsl:for-each> 
           </xsl:if></span></span></span></td>
      </xsl:if> 
      <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:AllowanceCharge">
       <td condition="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:AllowanceCharge" style="border: 0.05em solid rgb(132, 132, 132); width: 13%; text-align: right;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"> 
           <xsl:for-each select="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:AllowanceCharge"> 
            <table border="0" width="100%" cellspacing="0px" cellpadding="0px" style="border-collapse:collapse;"> 
             <tbody>
              <tr> 
               <td align="right"> <span> 
                 <xsl:text></xsl:text> 
                 <xsl:value-of select="format-number(./cbc:Amount, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
                 <xsl:if test="./cbc:Amount/@currencyID"> 
                  <xsl:text></xsl:text> 
                  <xsl:if test="./cbc:Amount/@currencyID = &apos;TRY&apos;"> 
                   <xsl:text>
                    TL
                   </xsl:text> 
                  </xsl:if> 
                  <xsl:if test="./cbc:Amount/@currencyID != &apos;TRY&apos;"> 
                   <xsl:value-of select="./cbc:Amount/@currencyID"></xsl:value-of> 
                  </xsl:if> 
                 </xsl:if> </span> </td> 
              </tr> 
             </tbody>
            </table> 
           </xsl:for-each>   </span></span></span></td>
      </xsl:if> 
      <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme">
       <td condition="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme" style="border: 0.05em solid rgb(132, 132, 132); width: 13%; text-align: right;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
           <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme">  
            <table border="0" width="100%" cellspacing="0px" cellpadding="0px" style="border-collapse:collapse;"> 
             <tbody> 
              <tr> 
               <td align="right"> <span> 
                 <xsl:text>&#160;
                 </xsl:text> 
                 <xsl:for-each select="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme"> 
                  <xsl:if test="cbc:TaxTypeCode=&apos;0015&apos; "> 
                   <xsl:text /> 
                   <xsl:if test="../../cbc:Percent"> 
                    <xsl:text>
                      %
                    </xsl:text> 
                    <xsl:value-of select="format-number(../../cbc:Percent, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
                   </xsl:if> 
                  </xsl:if> 
                 </xsl:for-each> </span> </td> 
              </tr> 
             </tbody> 
            </table> 
           </xsl:if></span></span></span></td>
      </xsl:if> 
      <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme">
       <td condition="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme" style="border: 0.05em solid rgb(132, 132, 132); width: 13%; text-align: right;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"> 
           <table border="0" width="100%" cellspacing="0px" cellpadding="0px" style="border-collapse:collapse;"> 
            <tbody> 
             <tr> 
              <td align="right"> <span> 
                <xsl:text>&#160;
                </xsl:text> 
                <xsl:for-each select="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme"> 
                 <xsl:if test="cbc:TaxTypeCode=&apos;0015&apos; "> 
                  <xsl:text /> 
                  <xsl:value-of select="format-number(../../cbc:TaxAmount, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
                  <xsl:if test="../../cbc:TaxAmount/@currencyID"> 
                   <xsl:text /> 
                   <xsl:if test="../../cbc:TaxAmount/@currencyID = &apos;TRY&apos;"> 
                    <xsl:text>
                     TL
                    </xsl:text> 
                   </xsl:if> 
                   <xsl:if test="../../cbc:TaxAmount/@currencyID != &apos;TRY&apos;"> 
                    <xsl:value-of select="../../cbc:TaxAmount/@currencyID" /> 
                   </xsl:if> 
                  </xsl:if> 
                 </xsl:if> 
                </xsl:for-each> </span> </td> 
             </tr> 
            </tbody> 
           </table>  </span></span></span></td>
      </xsl:if> 
      <xsl:if test="(//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal and //n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme and //n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode!=&apos;0015&apos;) or (//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:WithholdingTaxTotal/cac:TaxSubtotal)">
       <td condition="(//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal and //n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme and //n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode!=&apos;0015&apos;) or (//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:WithholdingTaxTotal/cac:TaxSubtotal)" style="border: 0.05em solid rgb(132, 132, 132); width: 13%; text-align: right;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
           <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal  
                    or //n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:WithholdingTaxTotal/cac:TaxSubtotal">  
            <xsl:if test="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme and ./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode!=&apos;0015&apos;"> 
            </xsl:if>
            <xsl:for-each select="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:WithholdingTaxTotal/cac:TaxSubtotal"> 
             <xsl:if test="not(cbc:TaxAmount = 0)"> 
              <xsl:choose> 
               <xsl:when test="cac:TaxCategory/cac:TaxScheme/cbc:Name=&apos;&apos;"> 
                <xsl:text>
                 Diger Vergiler Toplami 
                </xsl:text> 
               </xsl:when> 
               <xsl:otherwise> 
                <xsl:text>
                 Tevkifat (
                </xsl:text> 
                <xsl:value-of select="cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode"></xsl:value-of> 
                <xsl:text>
                 -
                </xsl:text> 
                <xsl:value-of select="cac:TaxCategory/cac:TaxScheme/cbc:Name"></xsl:value-of> 
                <xsl:text>
                 )
                </xsl:text> 
               </xsl:otherwise> 
              </xsl:choose> 
              <xsl:if test="cbc:Percent !=&apos;&apos;"> 
               <xsl:text>
                (%
               </xsl:text> 
               <xsl:value-of select=" format-number(cbc:Percent, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
               <xsl:text>
                )= 
               </xsl:text> 
              </xsl:if> 
              <xsl:value-of select="format-number(../cbc:TaxAmount, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
              <xsl:if test="../cbc:TaxAmount/@currencyID"> 
               <xsl:if test="../cbc:TaxAmount/@currencyID = &apos;TRY&apos;"> 
                <xsl:text>
                 TL
                </xsl:text> 
               </xsl:if> 
               <xsl:if test="../cbc:TaxAmount/@currencyID != &apos;TRY&apos;"> 
                <xsl:value-of select="../cbc:TaxAmount/@currencyID"></xsl:value-of> 
               </xsl:if> 
              </xsl:if> 
             </xsl:if> 
            </xsl:for-each>
            <table border="0" width="100%" cellspacing="0px" cellpadding="0px" style="border-collapse:collapse;"> 
             <tbody> 
              <tr> 
               <td align="left"> <span> 
                 <xsl:for-each select="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme"> 
                  <xsl:if test="cbc:TaxTypeCode!=&apos;0015&apos; "> 
                   <xsl:text>
                     (
                   </xsl:text> 
                   <xsl:value-of select="cbc:Name"></xsl:value-of> 
                   <xsl:text>
                    )
                   </xsl:text> 
                   <xsl:if test="../../cbc:Percent"> 
                    <xsl:text>
                      (%
                    </xsl:text> 
                    <xsl:value-of select="format-number(../../cbc:Percent, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
                    <xsl:text>
                     )=
                    </xsl:text> 
                   </xsl:if> 
                   <xsl:value-of select="format-number(../../cbc:TaxAmount, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
                   <xsl:if test="../../cbc:TaxAmount/@currencyID"> 
                    <xsl:text></xsl:text> 
                    <xsl:if test="../../cbc:TaxAmount/@currencyID = &apos;TRY&apos;"> 
                     <xsl:text>
                      TL
                     </xsl:text> 
                    </xsl:if> 
                    <xsl:if test="../../cbc:TaxAmount/@currencyID != &apos;TRY&apos;"> 
                     <xsl:value-of select="../../cbc:TaxAmount/@currencyID"></xsl:value-of> 
                    </xsl:if> 
                   </xsl:if> 
                  </xsl:if> 
                 </xsl:for-each> </span> </td> 
              </tr>   
             </tbody> 
            </table> 
           </xsl:if></span></span></span></td>
      </xsl:if> 
      <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:TaxExemptionReason">
       <td condition="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:TaxExemptionReason" style="border: 0.05em solid rgb(132, 132, 132); width: 13%; text-align: right;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"> 
           <table border="0" width="100%" cellspacing="0px" cellpadding="0px" style="border-collapse:collapse;"> 
            <tbody> 
             <tr> 
              <td> <span> 
                <xsl:if test="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:TaxExemptionReason"> 
                 <xsl:value-of select="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:TaxExemptionReason" /> 
                </xsl:if> </span> </td> 
             </tr> 
            </tbody> 
           </table>  </span></span></span></td>
      </xsl:if> 
      <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cbc:OutstandingQuantity">
       <td condition="//n1:DespatchAdvice/cac:DespatchLine/cbc:OutstandingQuantity" style="border: 0.05em solid rgb(132, 132, 132); width: 6%; text-align: right;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
           <xsl:value-of select="format-number(./cbc:OutstandingQuantity, &apos;###.##0,####&apos;, &apos;european&apos;)"> 
           </xsl:value-of>  
           <xsl:if test="./cbc:OutstandingQuantity/@unitCode"> 
            <xsl:for-each select="./cbc:DeliveredQuantity"> 
             <xsl:text> 
             </xsl:text> 
             <xsl:choose> 
              <xsl:when test="@unitCode  = &apos;26&apos;"> 
               <xsl:text>
                ton
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;BX&apos;"> 
               <xsl:text>
                Kutu
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;LTR&apos;"> 
               <xsl:text>
                lt
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;C62&apos;"> 
               <xsl:text>
                Adet
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;NIU&apos;"> 
               <xsl:text>
                Adet
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;KGM&apos;"> 
               <xsl:text>
                kg
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;KJO&apos;"> 
               <xsl:text>
                kJ
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;GRM&apos;"> 
               <xsl:text>
                g
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;MGM&apos;"> 
               <xsl:text>
                mg
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;NT&apos;"> 
               <xsl:text>
                Net Ton
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;GT&apos;"> 
               <xsl:text>
                Gross Ton
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;MTR&apos;"> 
               <xsl:text>
                m
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;MMT&apos;"> 
               <xsl:text>
                mm
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;KTM&apos;"> 
               <xsl:text>
                km
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;MLT&apos;"> 
               <xsl:text>
                ml
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;MMQ&apos;"> 
               <xsl:text>
                mm3
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;CLT&apos;"> 
               <xsl:text>
                cl
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;CMK&apos;"> 
               <xsl:text>
                cm2
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;CMQ&apos;"> 
               <xsl:text>
                cm3
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;CMT&apos;"> 
               <xsl:text>
                cm
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;MTK&apos;"> 
               <xsl:text>
                m2
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;MTQ&apos;"> 
               <xsl:text>
                m3
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;DAY&apos;"> 
               <xsl:text>
                 G&#252;n
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;MON&apos;"> 
               <xsl:text>
                 Ay
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;PA&apos;"> 
               <xsl:text>
                 Paket
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;KWH&apos;"> 
               <xsl:text>
                 KWH
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;ANN&apos;"> 
               <xsl:text>
                 Y&#305;l
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;HUR&apos;"> 
               <xsl:text>
                 Saat
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;D61&apos;"> 
               <xsl:text>
                 Dakika
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;D62&apos;"> 
               <xsl:text>
                 Saniye
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;CCT&apos;"> 
               <xsl:text>
                 Ton ba&#351;.ta&#351;&#305;ma kap.
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;D30&apos;"> 
               <xsl:text>
                 Br&#252;t kalori
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;D40&apos;"> 
               <xsl:text>
                 1000 lt
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;LPA&apos;"> 
               <xsl:text>
                 saf alkol lt
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;B32&apos;"> 
               <xsl:text>
                 kg.m2
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;NCL&apos;"> 
               <xsl:text>
                 h&#252;cre adet
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;PR&apos;"> 
               <xsl:text>
                 &#199;ift
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;R9&apos;"> 
               <xsl:text>
                 1000 m3
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;SET&apos;"> 
               <xsl:text>
                 Set
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;T3&apos;"> 
               <xsl:text>
                 1000 adet
               </xsl:text> 
              </xsl:when> 
             </xsl:choose> 
            </xsl:for-each> 
           </xsl:if> </span></span></span></td>
      </xsl:if> 
      <td style="border: 0.05em solid rgb(132, 132, 132); width: 10.6%; text-align: right;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
          <xsl:value-of select="format-number(./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cbc:LineExtensionAmount, &apos;###.##0,########&apos;, &apos;european&apos;)" /> 
          <xsl:if test="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cbc:LineExtensionAmount/@currencyID"> 
           <xsl:text> 
           </xsl:text> 
           <xsl:if test="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cbc:LineExtensionAmount/@currencyID = &quot;TRY&quot; or ./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cbc:LineExtensionAmount/@currencyID = &quot;TRY&quot;"> 
            <xsl:text>
             TL
            </xsl:text> 
           </xsl:if> 
           <xsl:if test="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cbc:LineExtensionAmount/@currencyID != &quot;TRY&quot; and ./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cbc:LineExtensionAmount/@currencyID != &quot;TRY&quot;"> 
            <xsl:value-of select="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cbc:LineExtensionAmount/@currencyID" /> 
           </xsl:if> 
          </xsl:if> </span></span></span></td> 
      <xsl:if test="//n1:DespatchAdvice/cbc:ProfileID =&apos;HKSIRSALIYE&apos;">
       <td condition="//n1:DespatchAdvice/cbc:ProfileID =&apos;HKSIRSALIYE&apos;" style="border: 0.05em solid rgb(132, 132, 132); width: 10.6%; text-align: right;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
           <xsl:if test="//n1:DespatchAdvice/cbc:ProfileID =&apos;HKSIRSALIYE&apos;"> 
            <xsl:for-each select="cac:Item/cac:AdditionalItemIdentification"> 
             <xsl:if test="cbc:ID/@schemeID = &apos;KUNYENO&apos;"> 
              <xsl:value-of select="cbc:ID"></xsl:value-of> 
             </xsl:if> 
            </xsl:for-each>  
           </xsl:if></span></span></span></td>
      </xsl:if> 
     </tr>
    </xsl:for-each> 
   </tbody> 
  </table>
 </body>
</html><html>
 <head></head>
 <body>
  <table border="0" cellpadding="0" cellspacing="0" style="width: 800px;"> 
   <tbody> 
    <tr> 
     <td align="right"> 
      <table border="1" bordercolor="#a4a4a4" cellpadding="0" cellspacing="0" id="budgetContainerTable" style="width: 371px;" width="500"> 
       <tbody> 
        <tr align="right" id="budgetContainerTr"> 
         <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">Toplam Tutar</span></span></span></span></td> 
         <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:variable name="masrafDegeri"> 
              <xsl:choose> 
               <xsl:when test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentType=&apos;CHARGETOTALAMOUNT&apos;]"> 
                <xsl:value-of select="(n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentType=&apos;CHARGETOTALAMOUNT&apos;]/cbc:ID)"></xsl:value-of> 
               </xsl:when> 
               <xsl:otherwise>
                 0 
               </xsl:otherwise> 
              </xsl:choose> 
             </xsl:variable> 
             <xsl:value-of select="format-number(n1:DespatchAdvice/cac:Shipment/cac:GoodsItem/cbc:ValueAmount + $masrafDegeri, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
             <xsl:choose> 
              <xsl:when test="n1:DespatchAdvice/cac:Shipment/cac:GoodsItem/cbc:ValueAmount/@currencyID = &apos;TRY&apos;"> 
               <xsl:text>
                 TL
               </xsl:text> 
              </xsl:when> 
              <xsl:otherwise> 
               <xsl:text> 
               </xsl:text> 
               <xsl:value-of select="n1:DespatchAdvice/cac:Shipment/cac:GoodsItem/cbc:ValueAmount/@currencyID"></xsl:value-of> 
              </xsl:otherwise> 
             </xsl:choose></span></span></span></td> 
        </tr> 
        <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentType=&apos;CHARGETOTALAMOUNT&apos;]">
         <tr align="right" id="budgetContainerTr"> 
          <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">Toplam &#304;skonto</span></span></span></span></td> 
          <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"> 
              <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentType=&apos;CHARGETOTALAMOUNT&apos;]"> 
               <xsl:for-each select="//n1:DespatchAdvice/cac:AdditionalDocumentReference"> 
                <xsl:if test="cbc:DocumentType = &apos;ALLOWANCETOTALAMOUNTLEGAL&apos;"> 
                 <xsl:value-of select="format-number(cbc:ID, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
                </xsl:if> 
               </xsl:for-each> 
               <xsl:choose> 
                <xsl:when test="n1:DespatchAdvice/cac:Shipment/cac:GoodsItem/cbc:ValueAmount/@currencyID = &apos;TRY&apos;"> 
                 <xsl:text>
                   TL
                 </xsl:text> 
                </xsl:when> 
                <xsl:otherwise> 
                 <xsl:text> 
                 </xsl:text> 
                 <xsl:value-of select="n1:DespatchAdvice/cac:Shipment/cac:GoodsItem/cbc:ValueAmount/@currencyID" /> 
                </xsl:otherwise> 
               </xsl:choose> 
              </xsl:if></span></span></span></td> 
         </tr>
        </xsl:if> 
        <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal">
         <tr align="right" id="budgetContainerTr"> 
          <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><strong><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
               <xsl:for-each select="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal[generate-id(.)=generate-id(key(&apos;groupByTaxes&apos;, cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode)[1])]"> 
                <table id="taxSubtotalHeaderTable" cellpadding="0" cellspacing="0" width="100%" tagid="taxtable"> 
                 <tbody>
                  <tr align="right" style="width: 319px;"> 
                   <td width="100%" align="right"> <span style="font-weight:bold; "> 
                     <xsl:text>
                       Hesaplanan 
                     </xsl:text> 
                     <xsl:value-of select="cac:TaxCategory/cac:TaxScheme/cbc:Name"></xsl:value-of> 
                     <xsl:if test="cbc:Percent and cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode!=&apos;0015&apos;"> 
                      <xsl:text>
                        (%
                      </xsl:text> 
                      <xsl:value-of select=" format-number(cbc:Percent, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
                      <xsl:text>
                       )
                      </xsl:text> 
                     </xsl:if> </span> </td> 
                  </tr> 
                 </tbody>
                </table> 
               </xsl:for-each></span></span></span></strong></td> 
          <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"> 
              <xsl:for-each select="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal[generate-id(.)=generate-id(key(&apos;groupByTaxes&apos;, cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode)[1])]"> 
               <table id="taxSubtotalsTable" cellpadding="0" cellspacing="0" align="right" tagid="taxtable" style="clear:both" width="100%"> 
                <tbody>
                 <tr id="budgetContainerTr" align="right"> 
                  <td align="right"> 
                   <xsl:variable name="sumTaxAmount" select="sum(key(&apos;groupByTaxes&apos;, ./cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode)/cbc:TaxAmount)"></xsl:variable> 
                   <xsl:value-of select="format-number($sumTaxAmount,&apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
                   <xsl:choose> 
                    <xsl:when test="//n1:DespatchAdvice/cac:Shipment/cac:GoodsItem/cbc:ValueAmount/@currencyID = &apos;TRY&apos;"> 
                     <xsl:text>
                       TL
                     </xsl:text> 
                    </xsl:when> 
                    <xsl:otherwise> 
                     <xsl:text> 
                     </xsl:text> 
                     <xsl:value-of select="//n1:DespatchAdvice/cac:Shipment/cac:GoodsItem/cbc:ValueAmount/@currencyID"></xsl:value-of> 
                    </xsl:otherwise> 
                   </xsl:choose> </td> 
                 </tr> 
                </tbody>
               </table> 
              </xsl:for-each> </span></span></span></td> 
         </tr>
        </xsl:if> 
        <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:WithholdingTaxTotal">
         <tr align="right" id="budgetContainerTr"> 
          <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><strong><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
               <xsl:for-each select="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:WithholdingTaxTotal/cac:TaxSubtotal[generate-id(.)=generate-id(key(&apos;groupByTevkifat&apos;, cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode)[1])]"> 
                <table id="taxSubtotalHeaderTable" cellpadding="0" cellspacing="0" width="100%" tagid="taxtable"> 
                 <tbody>
                  <tr align="right" style="width: 319px;"> 
                   <td width="100%" align="right"> <span style="font-weight:bold; "> 
                     <xsl:text>
                      Tevkifat 
                     </xsl:text> 
                     <xsl:value-of select="cac:TaxCategory/cac:TaxScheme/cbc:Name" /> 
                     <xsl:if test="cbc:Percent"> 
                      <xsl:text>
                        (%
                      </xsl:text> 
                      <xsl:value-of select=" format-number(cbc:Percent, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
                      <xsl:text>
                       )
                      </xsl:text> 
                     </xsl:if> </span> </td> 
                  </tr> 
                 </tbody>
                </table> 
               </xsl:for-each></span></span></span></strong></td> 
          <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
              <xsl:for-each select="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:WithholdingTaxTotal/cac:TaxSubtotal[generate-id(.)=generate-id(key(&apos;groupByTevkifat&apos;, cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode)[1])]"> 
               <table id="taxSubtotalsTable" cellpadding="0" cellspacing="0" align="right" tagid="taxtable" style="clear:both" width="100%"> 
                <tbody>
                 <tr id="budgetContainerTr" align="right"> 
                  <td align="right"> 
                   <xsl:variable name="sumTaxAmountTevkifat" select="sum(key(&apos;groupByTevkifat&apos;, ./cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode)/cbc:TaxAmount)"></xsl:variable> 
                   <xsl:value-of select="format-number($sumTaxAmountTevkifat,&apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
                   <xsl:choose> 
                    <xsl:when test="//n1:DespatchAdvice/cac:Shipment/cac:GoodsItem/cbc:ValueAmount/@currencyID = &apos;TRY&apos;"> 
                     <xsl:text>
                       TL
                     </xsl:text> 
                    </xsl:when> 
                    <xsl:otherwise> 
                     <xsl:text> 
                     </xsl:text> 
                     <xsl:value-of select="//n1:DespatchAdvice/cac:Shipment/cac:GoodsItem/cbc:ValueAmount/@currencyID"></xsl:value-of> 
                    </xsl:otherwise> 
                   </xsl:choose> </td> 
                 </tr> 
                </tbody>
               </table> 
              </xsl:for-each></span></span></span></td> 
         </tr>
        </xsl:if> 
        <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentType=&apos;TAXINCLUSIVEAMOUNTLEGAL&apos;]">
         <tr align="right" id="budgetContainerTr"> 
          <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">Vergiler Dahil Toplam Tutar</span></span></span></span></td> 
          <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"> 
              <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentType=&apos;TAXINCLUSIVEAMOUNTLEGAL&apos;]"> 
               <xsl:for-each select="//n1:DespatchAdvice/cac:AdditionalDocumentReference"> 
                <xsl:if test="cbc:DocumentType = &apos;TAXINCLUSIVEAMOUNTLEGAL&apos;"> 
                 <xsl:value-of select="format-number(cbc:ID, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
                </xsl:if> 
               </xsl:for-each> 
               <xsl:text>&#160;
               </xsl:text> 
               <xsl:choose> 
                <xsl:when test="n1:DespatchAdvice/cac:Shipment/cac:GoodsItem/cbc:ValueAmount/@currencyID = &apos;TRY&apos;"> 
                 <xsl:text>
                   TL
                 </xsl:text> 
                </xsl:when> 
                <xsl:otherwise> 
                 <xsl:text> 
                 </xsl:text> 
                 <xsl:value-of select="n1:DespatchAdvice/cac:Shipment/cac:GoodsItem/cbc:ValueAmount/@currencyID" /> 
                </xsl:otherwise> 
               </xsl:choose> 
              </xsl:if></span></span></span></td> 
         </tr>
        </xsl:if> 
        <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentType=&apos;PAYABLEAMOUNTLEGAL&apos;]">
         <tr align="right" id="budgetContainerTr"> 
          <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">&#214;denecek Tutar</span></span></span></span></td> 
          <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"> 
              <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentType=&apos;PAYABLEAMOUNTLEGAL&apos;]"> 
               <xsl:for-each select="//n1:DespatchAdvice/cac:AdditionalDocumentReference"> 
                <xsl:if test="cbc:DocumentType = &apos;PAYABLEAMOUNTLEGAL&apos;"> 
                 <xsl:value-of select="format-number(cbc:ID, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
                </xsl:if> 
               </xsl:for-each> 
               <xsl:text>&#160;
               </xsl:text> 
               <xsl:choose> 
                <xsl:when test="n1:DespatchAdvice/cac:Shipment/cac:GoodsItem/cbc:ValueAmount/@currencyID = &apos;TRY&apos;"> 
                 <xsl:text>
                   TL
                 </xsl:text> 
                </xsl:when> 
                <xsl:otherwise> 
                 <xsl:text> 
                 </xsl:text> 
                 <xsl:value-of select="n1:DespatchAdvice/cac:Shipment/cac:GoodsItem/cbc:ValueAmount/@currencyID" /> 
                </xsl:otherwise> 
               </xsl:choose> 
              </xsl:if></span></span></span></td> 
         </tr>
        </xsl:if> 
       </tbody> 
      </table> 
      <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentTypeCode=&apos;TARGETCURRENCY&apos;]">
       <table border="1" bordercolor="#a4a4a4" cellpadding="0" cellspacing="0" id="budgetContainerTable" style="width: 371px;" width="500"> 
        <tbody> 
         <tr align="right" id="budgetContainerTr"> 
          <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">D&#246;viz Kuru</span></span></span></span></td> 
          <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
              <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentTypeCode=&apos;TARGETCURRENCY&apos;]"> 
               <xsl:value-of select="format-number(//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentDescription=&apos;DOVIZKURU&apos;]/cbc:DocumentTypeCode, &apos;###.##0,0000&apos;, &apos;european&apos;)" /> 
               <xsl:text>&#160;
               </xsl:text> 
               <xsl:value-of select="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentTypeCode=&apos;TARGETCURRENCY&apos;]/cbc:ID" /> 
              </xsl:if></span></span></span></td> 
         </tr> 
         <tr align="right" id="budgetContainerTr"> 
          <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">Toplam Tutar</span></span></span></span></td> 
          <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
              <xsl:for-each select="//n1:DespatchAdvice/cac:AdditionalDocumentReference"> 
               <xsl:if test="cbc:DocumentType = &apos;LINEEXTENSIONAMOUNT&apos;"> 
                <xsl:value-of select="format-number(cbc:ID, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
               </xsl:if> 
              </xsl:for-each> 
              <xsl:text>&#160;
              </xsl:text> 
              <xsl:value-of select="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentTypeCode=&apos;TARGETCURRENCY&apos;]/cbc:ID"></xsl:value-of> </span></span></span></td> 
         </tr> 
         <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentType=&apos;CHARGETOTALAMOUNT&apos;]">
          <tr align="right" id="budgetContainerTr"> 
           <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">Toplam &#304;skonto</span></span></span></span></td> 
           <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
               <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentTypeCode=&apos;TARGETCURRENCY&apos;]"> 
                <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentType=&apos;CHARGETOTALAMOUNT&apos;]"> 
                 <span> 
                  <xsl:for-each select="//n1:DespatchAdvice/cac:AdditionalDocumentReference"> 
                   <xsl:if test="cbc:DocumentType = &apos;ALLOWANCETOTALAMOUNT&apos;"> 
                    <xsl:value-of select="format-number(cbc:ID, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
                   </xsl:if> 
                  </xsl:for-each> 
                  <xsl:text>&#160;
                  </xsl:text> 
                  <xsl:value-of select="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentTypeCode=&apos;TARGETCURRENCY&apos;]/cbc:ID" /> </span> 
                </xsl:if> 
               </xsl:if></span></span></span></td> 
          </tr>
         </xsl:if> 
         <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal">
          <tr align="right" id="budgetContainerTr"> 
           <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><strong><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
                <xsl:for-each select="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal[generate-id(.)=generate-id(key(&apos;groupByTaxes&apos;, cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode)[1])]"> 
                 <table id="taxSubtotalHeaderTable" cellpadding="0" cellspacing="0" width="100%" tagid="taxtable"> 
                  <tbody>
                   <tr align="right" style="width: 319px;"> 
                    <td width="100%" align="right"> <span style="font-weight:bold; "> <span style="font-weight:bold;"> 
                       <xsl:text>
                        Hesaplanan 
                       </xsl:text> 
                       <xsl:value-of select="cac:TaxCategory/cac:TaxScheme/cbc:Name" /> 
                       <xsl:if test="cbc:Percent and cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode!=&apos;0015&apos;"> 
                        <xsl:text>
                          (%
                        </xsl:text> 
                        <xsl:value-of select=" format-number(cbc:Percent, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
                        <xsl:text>
                         )
                        </xsl:text> 
                       </xsl:if> </span> </span></td> 
                   </tr> 
                  </tbody>
                 </table> 
                </xsl:for-each></span></span></span></strong></td> 
           <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
               <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentTypeCode=&apos;TARGETCURRENCY&apos;]"> 
                <xsl:for-each select="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal[generate-id(.)=generate-id(key(&apos;groupByTaxes&apos;, cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode)[1])]"> 
                 <table id="taxSubtotalsTable" cellpadding="0" cellspacing="0" align="right" tagid="taxtable" style="clear:both" width="100%"> 
                  <tbody>
                   <tr id="budgetContainerTr" align="right"> 
                    <td align="right"> 
                     <xsl:variable name="sumTaxAmount" select="sum(key(&apos;groupByTaxes&apos;, ./cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode)/cbc:TaxAmount)"></xsl:variable> 
                     <xsl:choose> 
                      <xsl:when test="cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode=&apos;0015&apos;"> 
                       <xsl:value-of select="format-number($sumTaxAmount*$CurrValue, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
                      </xsl:when> 
                      <xsl:otherwise> 
                       <xsl:variable name="var_Percent" select="cbc:Percent"></xsl:variable> 
                       <xsl:variable name="var_TaxTypeCode" select="cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode"></xsl:variable> 
                       <xsl:variable name="var_TaxName" select="cac:TaxCategory/cac:TaxScheme/cbc:Name"></xsl:variable> 
                       <xsl:for-each select="//n1:DespatchAdvice/cac:AdditionalDocumentReference"> 
                        <xsl:if test="(cbc:DocumentTypeCode = $var_TaxTypeCode) and (not(cbc:ID=0))"> 
                         <xsl:choose> 
                          <xsl:when test="(cbc:DocumentType = $var_Percent) and (cbc:DocumentDescription = $var_TaxName)"> 
                           <xsl:value-of select="format-number(cbc:ID, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
                          </xsl:when> 
                          <xsl:otherwise> 
                           <xsl:if test="not($var_Percent) and not(cbc:DocumentType) and (cbc:DocumentDescription = $var_TaxName)"> 
                            <xsl:value-of select="format-number(cbc:ID, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
                           </xsl:if> 
                          </xsl:otherwise> 
                         </xsl:choose> 
                        </xsl:if> 
                       </xsl:for-each> 
                      </xsl:otherwise> 
                     </xsl:choose> 
                     <xsl:text></xsl:text> 
                     <xsl:text>&#160;
                     </xsl:text> 
                     <xsl:value-of select="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentTypeCode=&apos;TARGETCURRENCY&apos;]/cbc:ID"></xsl:value-of> </td> 
                   </tr> 
                  </tbody>
                 </table> 
                </xsl:for-each> 
               </xsl:if></span></span></span></td> 
          </tr>
         </xsl:if> 
         <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:WithholdingTaxTotal">
          <tr align="right" id="budgetContainerTr"> 
           <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><strong><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
                <xsl:for-each select="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:WithholdingTaxTotal/cac:TaxSubtotal[generate-id(.)=generate-id(key(&apos;groupByTevkifat&apos;, cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode)[1])]"> 
                 <table id="taxSubtotalHeaderTable" cellpadding="0" cellspacing="0" width="100%" tagid="taxtable"> 
                  <tbody>
                   <tr align="right" style="width: 319px;"> 
                    <td width="100%" align="right"> <span style="font-weight:bold;"> 
                      <xsl:text>
                       Tevkifat 
                      </xsl:text> 
                      <xsl:value-of select="cac:TaxCategory/cac:TaxScheme/cbc:Name" /> 
                      <xsl:if test="cbc:Percent"> 
                       <xsl:text>
                         (%
                       </xsl:text> 
                       <xsl:value-of select=" format-number(cbc:Percent, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
                       <xsl:text>
                        )
                       </xsl:text> 
                      </xsl:if> </span> </td> 
                   </tr> 
                  </tbody>
                 </table> 
                </xsl:for-each></span></span></span></strong></td> 
           <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"> 
               <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentTypeCode=&apos;TARGETCURRENCY&apos;]"> 
                <xsl:for-each select="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:WithholdingTaxTotal/cac:TaxSubtotal[generate-id(.)=generate-id(key(&apos;groupByTevkifat&apos;, cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode)[1])]"> 
                 <table id="taxSubtotalsTable" cellpadding="0" cellspacing="0" align="right" tagid="taxtable" style="clear:both" width="100%"> 
                  <tbody>
                   <tr id="budgetContainerTr" align="right"> 
                    <td align="right"> 
                     <xsl:variable name="sumTaxAmountTevkifat" select="sum(key(&apos;groupByTevkifat&apos;, ./cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode)/cbc:TaxAmount)"></xsl:variable> 
                     <xsl:text></xsl:text> 
                     <xsl:variable name="var_Percent" select="cbc:Percent"></xsl:variable> 
                     <xsl:variable name="var_TaxTypeCode" select="cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode"></xsl:variable> 
                     <xsl:for-each select="//n1:DespatchAdvice/cac:AdditionalDocumentReference"> 
                      <xsl:if test="cbc:DocumentTypeCode = $var_TaxTypeCode"> 
                       <xsl:value-of select="format-number(cbc:ID, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
                      </xsl:if> 
                     </xsl:for-each> 
                     <xsl:text>&#160;
                     </xsl:text> 
                     <xsl:value-of select="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentTypeCode=&apos;TARGETCURRENCY&apos;]/cbc:ID"></xsl:value-of> </td> 
                   </tr> 
                  </tbody>
                 </table> 
                </xsl:for-each> 
               </xsl:if></span></span></span></td> 
          </tr>
         </xsl:if> 
         <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentType=&apos;TAXINCLUSIVEAMOUNT&apos;]">
          <tr align="right" id="budgetContainerTr"> 
           <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">Vergiler Dahil Toplam Tutar</span></span></span></span></td> 
           <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
               <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentTypeCode=&apos;TARGETCURRENCY&apos;]"> 
                <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentType=&apos;TAXINCLUSIVEAMOUNT&apos;]"> 
                 <xsl:for-each select="//n1:DespatchAdvice/cac:AdditionalDocumentReference"> 
                  <xsl:if test="cbc:DocumentType = &apos;TAXINCLUSIVEAMOUNT&apos;"> 
                   <xsl:value-of select="format-number(cbc:ID, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
                  </xsl:if> 
                 </xsl:for-each> 
                 <xsl:text>&#160;
                 </xsl:text> 
                 <xsl:value-of select="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentTypeCode=&apos;TARGETCURRENCY&apos;]/cbc:ID" /> 
                </xsl:if> 
               </xsl:if></span></span></span></td> 
          </tr>
         </xsl:if> 
         <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentType=&apos;PAYABLEAMOUNT&apos;]">
          <tr align="right" id="budgetContainerTr"> 
           <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">&#214;denecek Tutar</span></span></span></span></td> 
           <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
               <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentTypeCode=&apos;TARGETCURRENCY&apos;]"> 
                <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentType=&apos;PAYABLEAMOUNT&apos;]"> 
                 <xsl:for-each select="//n1:DespatchAdvice/cac:AdditionalDocumentReference"> 
                  <xsl:if test="cbc:DocumentType = &apos;PAYABLEAMOUNT&apos;"> 
                   <xsl:value-of select="format-number(cbc:ID, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
                  </xsl:if> 
                 </xsl:for-each> 
                 <xsl:text>&#160;
                 </xsl:text> 
                 <xsl:value-of select="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentTypeCode=&apos;TARGETCURRENCY&apos;]/cbc:ID" /> 
                </xsl:if> 
               </xsl:if></span></span></span></td> 
          </tr>
         </xsl:if> 
        </tbody> 
       </table>
      </xsl:if> </td> 
    </tr> 
   </tbody> 
  </table> 
  <table style="border: 2px solid rgb(0, 0, 0); width: 800px; border-collapse: collapse;"> 
   <thead> 
    <tr> 
     <th colspan="4">&#304;lgili D&#246;k&#252;manlar</th> 
    </tr> 
   </thead> 
   <tbody> 
    <tr> 
     <td style="border: 0.05em solid rgb(132, 132, 132); text-align: center;"><span style="font-size: 9px;"><strong><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;">Dok&#252;man No</span></span></strong></span></td> 
     <td style="border: 0.05em solid rgb(132, 132, 132); text-align: center;"><span style="font-size: 9px;"><strong><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;">Tarih</span></span></strong></span></td> 
     <td style="border: 0.05em solid rgb(132, 132, 132); text-align: center;"><span style="font-size: 9px;"><strong><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;">Dok&#252;man Tipi</span></span></strong></span></td> 
     <td style="border: 0.05em solid rgb(132, 132, 132); text-align: center;"><span style="font-size: 9px;"><strong><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;">A&#231;&#305;klama</span></span></strong></span></td> 
    </tr> 
    <tr loop="loop"> 
     <td style="border: 0.05em solid rgb(132, 132, 132); text-align: center;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
         <xsl:value-of select="./cbc:ID" /> </span></span></span></td> 
     <td style="border: 0.05em solid rgb(132, 132, 132); text-align: center;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
         <xsl:value-of select="./cbc:IssueDate"></xsl:value-of></span></span></span></td> 
     <td style="border: 0.05em solid rgb(132, 132, 132); text-align: center;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
         <xsl:value-of select="./cbc:DocumentType" /></span></span></span></td> 
     <td style="border: 0.05em solid rgb(132, 132, 132); text-align: center;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
         <xsl:value-of select="./cbc:DocumentDescription" /></span></span></span></td> 
    </tr> 
   </tbody> 
  </table> 
  <table style="border: 2px solid rgb(0, 0, 0); width: 800px; border-collapse: collapse;"> 
   <thead> 
    <tr> 
     <th colspan="2">Ta&#351;&#305;y&#305;c&#305; Bilgileri</th> 
    </tr> 
   </thead> 
   <tbody> 
    <tr> 
     <td style="border: 0.05em solid rgb(132, 132, 132); width: 372px;"><span style="font-size: 10px;"><span style="color: #696969; font-family: verdana, geneva, sans-serif;"><b>Ta&#351;&#305;y&#305;c&#305; Firma VKN/TCKN, Unvan:</b></span></span></td> 
     <td style="border: 0.05em solid rgb(132, 132, 132); width: 419px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
         <xsl:for-each select="//cac:CarrierParty"> 
          <xsl:choose> 
           <xsl:when test="string-length(./cac:PartyIdentification/cbc:ID) = 11"> 
            <xsl:value-of select="./cac:PartyIdentification/cbc:ID" /> , 
            <xsl:value-of select="./cac:Person/cbc:FirstName" />&#160;
            <xsl:value-of select="./cac:Person/cbc:FamilyName" /> 
           </xsl:when> 
           <xsl:when test="string-length(./cac:PartyIdentification/cbc:ID) = 10"> 
            <xsl:value-of select="./cac:PartyIdentification/cbc:ID" /> , 
            <xsl:value-of select="./cac:PartyName/cbc:Name" /> 
           </xsl:when> 
          </xsl:choose> 
         </xsl:for-each></span></span></span></td> 
    </tr> 
    <tr> 
     <td style="border: 0.05em solid rgb(132, 132, 132); width: 372px;"><span style="font-size: 9px;"><b style="color: #696969; font-family: verdana, geneva, sans-serif; font-size: x-small;">Ara&#231; Plakas&#305; Numaras&#305;:</b></span></td> 
     <td style="border: 0.05em solid rgb(132, 132, 132); width: 419px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
         <xsl:for-each select="//cac:ShipmentStage/cac:TransportMeans/cac:RoadTransport/cbc:LicensePlateID"> 
          <xsl:value-of select="." /> 
         </xsl:for-each> </span></span></span></td> 
    </tr> 
    <tr> 
     <td style="border: 0.05em solid rgb(132, 132, 132); width: 372px;"><span style="font-size: 9px;"><b style="color: #696969; font-family: verdana, geneva, sans-serif; font-size: x-small;">Dorse Plakas&#305; Numaras&#305;:</b></span></td> 
     <td style="border: 0.05em solid rgb(132, 132, 132); width: 419px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
         <xsl:for-each select="//cac:TransportHandlingUnit/cac:TransportEquipment/cbc:ID[@schemeID = &apos;DORSEPLAKA&apos;]"> 
          <xsl:value-of select="." /> 
         </xsl:for-each> </span></span></span></td> 
    </tr> 
    <tr> 
     <td style="border: 0.05em solid rgb(132, 132, 132); width: 372px;"><span style="font-size: 9px;"><b style="color: #696969; font-family: verdana, geneva, sans-serif; font-size: x-small;">&#350;&#246;der Ad/Soyad, TCKN:</b></span></td> 
     <td style="border: 0.05em solid rgb(132, 132, 132); width: 419px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
         <xsl:for-each select="//cac:ShipmentStage/cac:DriverPerson"> 
          <xsl:if test="cbc:FirstName"> 
           <xsl:for-each select="cbc:Title"> 
            <xsl:apply-templates /> 
            <xsl:text>&#160;
            </xsl:text> 
           </xsl:for-each> 
           <xsl:for-each select="cbc:FirstName"> 
            <xsl:apply-templates /> 
            <xsl:text>&#160;
            </xsl:text> 
           </xsl:for-each> 
           <xsl:for-each select="cbc:MiddleName"> 
            <xsl:apply-templates /> 
            <xsl:text>&#160;
            </xsl:text> 
           </xsl:for-each> 
           <xsl:for-each select="cbc:FamilyName"> 
            <xsl:apply-templates /> 
           </xsl:for-each>, 
           <xsl:for-each select="cbc:NationalityID"> 
            <xsl:apply-templates /> 
           </xsl:for-each> 
          </xsl:if> 
          <xsl:text>&#160;
          </xsl:text> 
         </xsl:for-each></span></span></span></td> 
    </tr> 
   </tbody> 
  </table> 
  <table style="border: 2px solid rgb(0, 0, 0); width: 800px; border-collapse: collapse;"> 
   <thead> 
    <tr> 
     <th colspan="2">A&#231;&#305;klamalar</th> 
    </tr> 
   </thead> 
   <tbody> 
    <tr> 
     <td style="border: 0.05em solid rgb(132, 132, 132); width: 373px;"><span style="font-size: 9px;"><b style="color: #696969; font-family: verdana, geneva, sans-serif; font-size: x-small;">Not:</b></span></td> 
     <td style="border: 0.05em solid rgb(132, 132, 132); width: 419px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
         <xsl:for-each select="//n1:DespatchAdvice/cbc:Note"> 
          <xsl:value-of select="." /> 
          <br /> 
         </xsl:for-each> </span></span></span></td> 
    </tr> 
   </tbody> 
  </table> 
  <table border="1" cellpadding="1" cellspacing="1" style="width: 800px;"> 
   <tbody> 
    <tr> 
     <td style="background-color: #000000; width: 236px;"><span style="color: #ffffff;"><strong><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;">Banka Ad&#305;</span></span></strong></span></td> 
     <td style="background-color: #000000; width: 139px;"><span style="color: #ffffff;"><strong><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;">Para Birimi</span></span></strong></span></td> 
     <td style="background-color: #000000;"><span style="color: #ffffff;"><strong><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;">Iban</span></span></strong></span></td> 
    </tr> 
    <tr> 
     <td style="width: 236px;"><strong><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;"><span style="color: #000000;">AKBANK</span></span></span></strong></td> 
     <td style="width: 139px;"><strong><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;"><span style="color: #000000;">TL</span></span></span></strong></td> 
     <td><strong><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;"><span style="color: #000000;">TR46 0004 6006 2688 8000 2108 03</span></span></span></strong></td> 
    </tr> 
    <tr> 
     <td style="width: 236px;"><strong><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;"><span style="color: #000000;">KUVEYTTURK</span></span></span></strong></td> 
     <td style="width: 139px;"><strong><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;"><span style="color: #000000;">TL</span></span></span></strong></td> 
     <td><strong><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;"><span style="color: #000000;">TR04 0020 5000 0993 8161 8000 01</span></span></span></strong></td> 
    </tr> 
    <tr> 
     <td style="width: 236px;"><strong><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;"><span style="color: #000000;">VAKIFKATILIM</span></span></span></strong></td> 
     <td style="width: 139px;"><strong><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;"><span style="color: #000000;">TL</span></span></span></strong></td> 
     <td><strong><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;"><span style="color: #000000;">TR30 0021 0000 0014 2034 5000 01<br /></span></span></span></strong></td> 
    </tr> 
   </tbody> 
  </table> 
  
 </body>
</html></xsl:template>
<xsl:template match="//n1:DespatchAdvice/cac:DespatchLine">
        <tr class="lineTableTr">
            <td class="lineTableTd">
                <xsl:text>&#160;</xsl:text>
                <xsl:value-of select="./cbc:ID"/>
            </td>
            <td class="lineTableTd">
                <xsl:text>&#160;</xsl:text>
                <xsl:value-of select="./cac:Item/cbc:Name"/>
            </td>
            <td class="lineTableTd">
                <xsl:text>&#160;</xsl:text>
                <xsl:value-of select="./cac:Item/cbc:Description"/>
            </td>
            <td class="lineTableTd" align="right">
                <xsl:text>&#160;</xsl:text>
                <xsl:value-of
    select="format-number(./cbc:DeliveredQuantity, '###.###,####', 'european')"/>
                <xsl:if test="./cbc:DeliveredQuantity/@unitCode">
                    <xsl:for-each select="./cbc:DeliveredQuantity">
                        <xsl:text> </xsl:text>
                        <xsl:choose>
                            <xsl:when test="@unitCode  = 'SA'">
                                <xsl:text>ÇUVAL</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = '26'">
                                <xsl:text>ton</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'BX'">
                                <xsl:text>Kutu</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'LTR'">
                                <xsl:text>lt</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'C62'">
                                <xsl:text>Adet</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'NIU'">
                                <xsl:text>Adet</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'KGM'">
                                <xsl:text>kg</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'KJO'">
                                <xsl:text>kJ</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'GRM'">
                                <xsl:text>g</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'MGM'">
                                <xsl:text>mg</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'NT'">
                                <xsl:text>Net Ton</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'GT'">
                                <xsl:text>Gross Ton</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'MTR'">
                                <xsl:text>m</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'MMT'">
                                <xsl:text>mm</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'KTM'">
                                <xsl:text>km</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'MLT'">
                                <xsl:text>ml</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'MMQ'">
                                <xsl:text>mm3</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'CLT'">
                                <xsl:text>cl</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'CMK'">
                                <xsl:text>cm2</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'CMQ'">
                                <xsl:text>cm3</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'CMT'">
                                <xsl:text>cm</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'MTK'">
                                <xsl:text>m2</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'MTQ'">
                                <xsl:text>m3</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'DAY'">
                                <xsl:text> Gün</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'MON'">
                                <xsl:text> Ay</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'PA'">
                                <xsl:text> Paket</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'KWH'">
                                <xsl:text> KWH</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'ANN'">
                                <xsl:text> Yıl</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'HUR'">
                                <xsl:text> Saat</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'D61'">
                                <xsl:text> Dakika</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'D62'">
                                <xsl:text> Saniye</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'CCT'">
                                <xsl:text> Ton baş.taşıma kap.</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'D30'">
                                <xsl:text> Brüt kalori</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'D40'">
                                <xsl:text> 1000 lt</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'LPA'">
                                <xsl:text> saf alkol lt</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'B32'">
                                <xsl:text> kg.m2</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'NCL'">
                                <xsl:text> hücre adet</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'PR'">
                                <xsl:text> Çift</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'R9'">
                                <xsl:text> 1000 m3</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'SET'">
                                <xsl:text> Set</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'T3'">
                                <xsl:text> 1000 adet</xsl:text>
                            </xsl:when>
                            <xsl:when test="@unitCode  = 'D64'">
                                <xsl:text> BLOK</xsl:text>
                            </xsl:when>
                        </xsl:choose>
                    </xsl:for-each>
                </xsl:if>
            </td>
            <td class="lineTableTd" align="right">
                <xsl:text>&#160;</xsl:text>
                <xsl:value-of
    select="format-number(./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:Price/cbc:PriceAmount, '###.##0,########', 'european')"/>
                <xsl:if test="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:Price/cbc:PriceAmount/@currencyID">
                    <xsl:text> </xsl:text>
                    <xsl:if test="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:Price/cbc:PriceAmount/@currencyID = &quot;TRL&quot; or ./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:Price/cbc:PriceAmount/@currencyID = &quot;TRY&quot;">
                        <xsl:text>TL</xsl:text>
                    </xsl:if>
                    <xsl:if test="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:Price/cbc:PriceAmount/@currencyID != &quot;TRL&quot; and ./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:Price/cbc:PriceAmount/@currencyID != &quot;TRY&quot;">
                        <xsl:value-of select="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:Price/cbc:PriceAmount/@currencyID"/>
                    </xsl:if>
                </xsl:if>
            </td>
            <td class="lineTableTd" align="right">
                <xsl:text>&#160;</xsl:text>
                <xsl:value-of
    select="format-number(./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cbc:LineExtensionAmount, '###.##0,########', 'european')"/>
                <xsl:if test="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cbc:LineExtensionAmount/@currencyID">
                    <xsl:text> </xsl:text>
                    <xsl:if test="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cbc:LineExtensionAmount/@currencyID = &quot;TRL&quot; or ./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cbc:LineExtensionAmount/@currencyID = &quot;TRY&quot;">
                        <xsl:text>TL</xsl:text>
                    </xsl:if>
                    <xsl:if test="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cbc:LineExtensionAmount/@currencyID != &quot;TRL&quot; and ./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cbc:LineExtensionAmount/@currencyID != &quot;TRY&quot;">
                        <xsl:value-of select="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cbc:LineExtensionAmount/@currencyID"/>
                    </xsl:if>
                </xsl:if>
            </td>
        </tr>
    </xsl:template>
    <xsl:template match="//cbc:IssueDate">
        <xsl:value-of select="substring(.,9,2)"/>-<xsl:value-of select="substring(.,6,2)"/>-<xsl:value-of select="substring(.,1,4)"/>
    </xsl:template>
    <xsl:template match="//cbc:ActualDespatchDate">
        <xsl:value-of select="substring(.,9,2)"/>-<xsl:value-of select="substring(.,6,2)"/>-<xsl:value-of select="substring(.,1,4)"/>
    </xsl:template>
    <xsl:template match="//n1:DespatchAdvice">
        <tr class="lineTableTr">
            <td class="lineTableTd">
                <xsl:text>&#160;</xsl:text>
            </td>
            <td class="lineTableTd">
                <xsl:text>&#160;</xsl:text>
            </td>
            <td class="lineTableTd" align="right">
                <xsl:text>&#160;</xsl:text>
            </td>
            <td class="lineTableTd" align="right">
                <xsl:text>&#160;</xsl:text>
            </td>
            <td class="lineTableTd" align="right">
                <xsl:text>&#160;</xsl:text>
            </td>
            <td class="lineTableTd" align="right">
                <xsl:text>&#160;</xsl:text>
            </td>
        
        </tr>
    </xsl:template>
    <xsl:template name="Party_Title" >
        <xsl:param name="PartyType" />
        <td style="width:469px; " align="left">
            <xsl:if test="cac:PartyName">
                <xsl:value-of select="cac:PartyName/cbc:Name"/>
                <br/>
            </xsl:if>
            <xsl:for-each select="cac:Person">
                <xsl:for-each select="cbc:Title">
                    <xsl:apply-templates/>
                    <xsl:text>&#160;</xsl:text>
                </xsl:for-each>
                <xsl:for-each select="cbc:FirstName">
                    <xsl:apply-templates/>
                    <xsl:text>&#160;</xsl:text>
                </xsl:for-each>
                <xsl:for-each select="cbc:MiddleName">
                    <xsl:apply-templates/>
                    <xsl:text>&#160; </xsl:text>
                </xsl:for-each>
                <xsl:for-each select="cbc:FamilyName">
                    <xsl:apply-templates/>
                    <xsl:text>&#160;</xsl:text>
                </xsl:for-each>
                <xsl:for-each select="cbc:NameSuffix">
                    <xsl:apply-templates/>
                </xsl:for-each>
                <xsl:if test="$PartyType='TAXFREE'">
                    <br/>
                    <xsl:text>Pasaport No: </xsl:text>
                    <xsl:value-of select="cac:IdentityDocumentReference/cbc:ID"/>
                    <br/>
                    <xsl:text>Ülkesi: </xsl:text>
                    <xsl:value-of select="cbc:NationalityID"/>
                </xsl:if>
            </xsl:for-each>
        </td>
    </xsl:template>
    <xsl:template name="Party_Adress" >
        <xsl:param name="PartyType" />
        <td style="width:469px; " align="left">
            <xsl:for-each select="cac:PostalAddress">
                <xsl:for-each select="cbc:StreetName">
                    <xsl:apply-templates/>
                    <xsl:text>&#160;</xsl:text>
                </xsl:for-each>
                <xsl:for-each select="cbc:BuildingName">
                    <xsl:apply-templates/>
                </xsl:for-each>
                <xsl:for-each select="cbc:BuildingNumber">
                    <xsl:text> No:</xsl:text>
                    <xsl:apply-templates/>
                    <xsl:text>&#160;</xsl:text>
                </xsl:for-each>
                <br/>
                <xsl:for-each select="cbc:Room">
                    <xsl:text>Kapı No:</xsl:text>
                    <xsl:apply-templates/>
                    <xsl:text>&#160;</xsl:text>
                </xsl:for-each>
                <br/>
                <xsl:for-each select="cbc:PostalZone">
                    <xsl:apply-templates/>
                    <xsl:text>&#160;</xsl:text>
                </xsl:for-each>
                <xsl:for-each select="cbc:CitySubdivisionName">
                    <xsl:apply-templates/>
                    <xsl:text>/ </xsl:text>
                </xsl:for-each>
                <xsl:for-each select="cbc:CityName">
                    <xsl:apply-templates/>
                    <xsl:text>&#160;</xsl:text>
                </xsl:for-each>
                <xsl:if test="$PartyType='TAXFREE'">
                    <br/>
                    <xsl:value-of select="cac:Country/cbc:Name"/>
                    <br/>
                </xsl:if>
            </xsl:for-each>
        </td>
    </xsl:template>
    <xsl:template name='Party_Other'>
        <xsl:param name="PartyType" />
        <xsl:for-each select="cbc:WebsiteURI">
            <tr align="left">
                <td>
                    <xsl:text>Web Sitesi: </xsl:text>
                    <xsl:value-of select="."/>
                </td>
            </tr>
        </xsl:for-each>
        <xsl:for-each select="cac:Contact/cbc:ElectronicMail">
            <tr align="left">
                <td>
                    <xsl:text>E-Posta: </xsl:text>
                    <xsl:value-of select="."/>
                </td>
            </tr>
        </xsl:for-each>
        <xsl:for-each select="cac:Contact">
            <xsl:if test="cbc:Telephone or cbc:Telefax">
                <tr align="left">
                    <td style="width:469px; " align="left">
                        <xsl:for-each select="cbc:Telephone">
                            <xsl:text>Tel: </xsl:text>
                            <xsl:apply-templates/>
                        </xsl:for-each>
                        <xsl:for-each select="cbc:Telefax">
                            <xsl:text> Fax: </xsl:text>
                            <xsl:apply-templates/>
                        </xsl:for-each>
                        <xsl:text>&#160;</xsl:text>
                    </td>
                </tr>
            </xsl:if>
        </xsl:for-each>
        <xsl:if test="$PartyType!='TAXFREE'">
            <xsl:for-each select="cac:PartyTaxScheme/cac:TaxScheme/cbc:Name">
                <tr align="left">
                    <td>
                        <xsl:text>Vergi Dairesi: </xsl:text>
                        <xsl:apply-templates/>
                    </td>
                </tr>
            </xsl:for-each>
            <xsl:for-each select="cac:PartyIdentification">
                <tr align="left">
                    <td>
                        <xsl:value-of select="cbc:ID/@schemeID"/>
                        <xsl:text>: </xsl:text>
                        <xsl:value-of select="cbc:ID"/>
                    </td>
                </tr>
            </xsl:for-each>
        </xsl:if>
    </xsl:template>
    <xsl:template name="Curr_Type">
        <xsl:value-of select="format-number(., '###.##0,00', 'european')"/>
        <xsl:if test="@currencyID">
            <xsl:text> </xsl:text>
            <xsl:choose>
                <xsl:when test="@currencyID = 'TRL' or @currencyID = 'TRY'">
                    <xsl:text>TL</xsl:text>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:value-of select="@currencyID"/>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:if>
    </xsl:template>
</xsl:stylesheet>