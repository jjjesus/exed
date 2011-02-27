<?xml version="1.0" encoding="iso-8859-1"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:nsm="http://foo.bar/responsecode.xsd">
    <xsl:template match="/">
        <html><body><h2>Responses</h2>

                        <xsl:for-each select="nsm:root/nsm:responses/nsm:response">
                                <xsl:choose>
                                        <xsl:when test="nsm:description != ''">
                                                <br/>'''&lt;description&gt;
                                                <br/>'''<xsl:value-of select="nsm:description" />
                                                <br/>'''&lt;/description&gt;
                                        </xsl:when>
                                </xsl:choose>
                                <br/>
                                <xsl:value-of select="@code" />

                        </xsl:for-each>
                </body>
        </html>
    </xsl:template>
</xsl:stylesheet>