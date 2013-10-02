<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  version="1.0"
  xmlns:wix="http://schemas.microsoft.com/wix/2006/wi">
  
  <xsl:param name="version"/>
  
  <xsl:template match="node()|@*">
    <xsl:copy>
      <xsl:apply-templates select="node()|@*"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="@Version[parent::wix:Product]">
    <xsl:attribute name="Version">
      <xsl:value-of select="$version"/>
    </xsl:attribute>
  </xsl:template>  

</xsl:stylesheet>