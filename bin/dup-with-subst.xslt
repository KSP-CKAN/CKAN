<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msb="http://schemas.microsoft.com/developer/msbuild/2003"
    xmlns="http://schemas.microsoft.com/developer/msbuild/2003">

    <!--
        XSLT template to add a new translation to CKAN-GUI.csproj.

        Usage (use double hyphen for stringparam, not allowed in XML comment):

            xsltproc -stringparam fromStr pt-BR -stringparam toStr ru-RU dup-with-subst.xslt CKAN-GUI.csproj
    -->

    <xsl:output method="xml" version="1.0" indent="yes" omit-xml-declaration="no"/>

    <xsl:strip-space elements="*"/>

    <xsl:param name="fromStr" select="fromStr"/>
    <xsl:param name="toStr" select="toStr"/>

    <xsl:template match="//msb:EmbeddedResource">
        <xsl:copy>
            <xsl:apply-templates select="node()|@*"/>
        </xsl:copy>
        <xsl:if test="contains(@Include, $fromStr)">
            <xsl:copy>
                <xsl:attribute name="Include">
                    <xsl:call-template name="replace">
                        <xsl:with-param name="haystack" select="@Include"/>
                        <xsl:with-param name="findWhat" select="$fromStr"/>
                        <xsl:with-param name="replaceWith" select="$toStr"/>
                    </xsl:call-template>
                </xsl:attribute>
                <xsl:if test="msb:LogicalName">
                    <LogicalName>
                        <xsl:call-template name="replace">
                            <xsl:with-param name="haystack" select="msb:LogicalName"/>
                            <xsl:with-param name="findWhat" select="$fromStr"/>
                            <xsl:with-param name="replaceWith" select="$toStr"/>
                        </xsl:call-template>
                    </LogicalName>
                </xsl:if>
                <xsl:apply-templates select="node()[not(self::msb:LogicalName)]"/>
            </xsl:copy>
        </xsl:if>
    </xsl:template>

    <xsl:template match="node()|@*">
        <xsl:copy>
            <xsl:apply-templates select="node()|@*"/>
        </xsl:copy>
    </xsl:template>

    <xsl:template name="replace">
        <xsl:param name="haystack"/>
        <xsl:param name="findWhat"/>
        <xsl:param name="replaceWith"/>
        <xsl:choose>
            <xsl:when test="contains($haystack, $findWhat)">
                <xsl:value-of select="substring-before($haystack, $findWhat)"/>
                <xsl:value-of select="$replaceWith"/>
                <xsl:call-template name="replace">
                    <xsl:with-param name="haystack" select="substring-after($haystack, $findWhat)"/>
                    <xsl:with-param name="findWhat" select="$findWhat"/>
                    <xsl:with-param name="replaceWith" select="$replaceWith"/>
                </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="$haystack"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

</xsl:stylesheet>
