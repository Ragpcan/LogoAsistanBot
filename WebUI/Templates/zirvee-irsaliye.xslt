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
  <table border="0" cellpadding="0" cellspacing="0" style="border-color: blue; width: 800px;" width="800"> 
   <tbody> 
    <tr> 
     <td colspan="2" width="60%" style="width: 497.25px;">&#160;</td> 
     <td align="right" width="40%" style="width: 299.194px;">&#160;</td> 
    </tr> 
    <tr height="3px;"></tr> 
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
     <td align="right" style="width: 242px;"> <p><img src="data:image/jpg;base64,/9j/4AAQSkZJRgABAQEAeAB4AAD/2wBDAAMCAgMCAgMDAwMEAwMEBQgFBQQEBQoHBwYIDAoMDAsKCwsNDhIQDQ4RDgsLEBYQERMUFRUVDA8XGBYUGBIUFRT/2wBDAQMEBAUEBQkFBQkUDQsNFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBT/wAARCAGSBlADASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD9U6KKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooqO4uIrS3lnnlSGCJS8ksjBVRQMkkngADvQNJt2RJWF4g8d+G/CbBdb8QaXo7kZC315HCSPYMQTXxn+0L+2Tqevahc6D4CvH03R4iY5NXhys90e/lnrGnoR8x65HSvlm4uJbueSaeV5ppCWeSRizMfUk9TXymLz6nSk4UI81uvT5dz9oyTw1xWNoxxGYVPZJ6qKV5fO9kvTV97M/WXRPiv4L8STpBpfizRb+4c4WCC/iaQ/8A3Z/Suqr8bK9s+Cf7VHir4U3ttaXtzNr3hoELJp9zJueFfWFzypH937p9BnIyw/EEZSUa8LLuv8jtzXwvq0aTqZbW52vsyVm/R7X9UvU/SeisXwd4x0jx94ds9c0O8S9066Xcki9Qe6sOqsDwQelbVfXRkpJSi7pn4bUpzozdOompJ2ae6fZhRRRVGYUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRUdxcRWlvLPPKkMESl5JZGCqigZJJPAAHegaTbsguLiK0t5Z55UhgiUvJLIwVUUDJJJ4AA718C/tSftSS/Ea4n8LeFp3h8LRNtuLpcq2oMD+YiB6D+LqewB+1J+1JL8Rrifwt4WneHwtE224ulyragwP5iIHoP4up7AfNVfCZtm3tb4fDv3er7+S8vz9N/6Q4J4JWD5czzOP7zeMX9nzf97svs+uxRRRXyR+3hRRRQB6n8BPj3q/wR8RebFvvdBumAv9NLcOOnmJnhZAO/foexH6TeDvGOkePvDtnrmh3iXunXS7kkXqD3Vh1VgeCD0r8hq9T+Anx71f4I+IvNi33ug3TAX+mluHHTzEzwsgHfv0PYj6PKs0eEfsqusH+H/APynjLg2GdweNwSSxCXymuz/vdn8npZr9QKKxfB3jHSPH3h2z1zQ7xL3TrpdySL1B7qw6qwPBB6VtV+hxkpJSi7pn8s1Kc6M3TqJqSdmnun2YUUUVRmFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFR3FxFaW8s88qQwRKXklkYKqKBkkk8AAd6BpNuyC4uIrS3lnnlSGCJS8ksjBVRQMkkngADvXwL+1J+1JL8Rrifwt4WneHwtE224ulyragwP5iIHoP4up7AH7Un7UkvxGuJ/C3had4fC0Tbbi6XKtqDA/mIgeg/i6nsB81V8Jm2be1vh8O/d6vv5Ly/P03/pDgnglYPlzPM4/vN4xf2fN/wB7svs+uxRRRXyR+3hRRRQAUUUUAFFFFAHqPwH+Pes/BDX2mtw1/od0R9t0xnwsnYOh/hcevccHtj9I/A3jnRviN4atNd0K7W7sLkcHo8bd0cfwsO4/oQa/IuvS/gZ8c9Z+CXiUXlmWu9IuSFv9NZsJMv8AeX+647N+B4NfRZXmssI1Sq6wf4f8DyPyvjHgynncHjMGlHEL5KaXR+fZ/J6Wa/UiisDwN450b4jeGrTXdCu1u7C5HB6PG3dHH8LDuP6EGt+v0SMlNKUXdM/lirSqUKkqVWLjKLs09GmujCiiiqMgooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKjuLiK0t5Z55UhgiUvJLIwVUUDJJJ4AA70DSbdkFxcRWlvLPPKkMESl5JZGCqigZJJPAAHevgX9qT9qSX4jXE/hbwtO8PhaJttxdLlW1BgfzEQPQfxdT2AP2pP2pJfiNcT+FvC07w+Fom23F0uVbUGB/MRA9B/F1PYD5qr4TNs29rfD4d+71ffyXl+fpv/SHBPBKwfLmeZx/ebxi/s+b/AL3ZfZ9diiiivkj9vCiiigAooooAKKKKACiiigAooooA9L+Bnxz1n4JeJReWZa70i5IW/wBNZsJMv95f7rjs34Hg1+lPgbxzo3xG8NWmu6Fdrd2FyOD0eNu6OP4WHcf0INfkXXpfwM+Oes/BLxKLyzLXekXJC3+ms2EmX+8v91x2b8Dwa+iyvNHhH7KrrB/h/wAA/LOMuDaeeU3jMGlHERXoprs/Ps/k9LNfqRRWB4G8c6N8RvDVpruhXa3dhcjg9Hjbujj+Fh3H9CDW/X6JGSmlKLumfyvVpVKFSVKrFxlF2aejTXRhRRRVGQUUUUAFFFFABRRRQAUUUUAFFFFABRRR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XaAEdsrIVYfkfrX51i8oxOHk+WLlHuv1R/VGS8cZTmtFOtVVKp1jJ2V/KT0a7de6PC6K9j0b9kT4q6xOqHwybGM9Zry6hjVfqNxb8ga+lfgn+xTpHge8g1nxdcw+IdWiIeKzjQ/Y4W9Tu5kI7ZAHseDWWHyvFYiVuRxXd6HbmnGWTZXSc3XjUl0jBqTb+Wi+ZxX7Kf7Kf2n7H418a2f7nibTdJnX7/dZpVPbuqnr1PGAftCiiv0XCYSng6fs6fzfc/lfPM8xef4t4rFP/DHpFdl+r6hRRRXafPBRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFAH/2Q==" style="width: 240px; " /></p> 
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
    <tr> 
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
     <td style="width: 236px;"><strong><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;"><span style="color: #000000;">Z&#304;RAAT BANKASI</span></span></span></strong></td> 
     <td style="width: 139px;"><strong><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;"><span style="color: #000000;">TL</span></span></span></strong></td> 
     <td><strong><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;"><span style="color: #000000;">TR40 0001 0023 4090 3610 7950 01</span></span></span></strong></td> 
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
		<xsl:if	test="@currencyID">
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