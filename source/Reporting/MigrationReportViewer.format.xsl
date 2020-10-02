<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="/">
    <html>
      <body>
        <h1> Configuration</h1>
        <table border="1" style="width:100%">
          <tr>
            <td>
              <b>Run Date: </b>
            </td>
            <td>
              <xsl:value-of select="/MigrationReport/Configuration/@Date"/>
            </td>
          </tr>
          <tr>
            <td>
              <b> Connection String:</b>
            </td>
            <td>
              <xsl:value-of select="/MigrationReport/Configuration/@ConnectionString"/>
            </td>
          </tr>
          <tr>
            <td>
              <b>Start Time:</b>
            </td>
            <td>
              <xsl:value-of select="/MigrationReport/Configuration/@StartTime"/>
            </td>
          </tr>
          <tr>

            <td>
              <b>End Time: </b>
            </td>
            <td>
              <xsl:value-of select="/MigrationReport/Configuration/@EndTime"/>
            </td>
          </tr>

        </table>
        <h1>Statistics</h1>
        <table border="1" style="width:100%">
          <tr>
            <td>Total </td>
            <td>Success</td>
            <td>Skipped</td>
            <td>Failed</td>
          </tr>
          <tr>
            <td>
              <xsl:value-of select="/MigrationReport/CounterStatistics/@TotalEntries" />
            </td>
            <td>
              <xsl:value-of select="/MigrationReport/CounterStatistics/@SuccessEntries" />
            </td>
            <td>
              <xsl:value-of select="/MigrationReport/CounterStatistics/@SkippedEntries" />
            </td>
            <td>
              <xsl:value-of select="/MigrationReport/CounterStatistics/@FailedEntries" />
            </td>
          </tr>


        </table>

        <h1> Summary </h1>
        <table border="1">
          <tr>
            <td>
              <b>Start Time</b>
            </td>
            <td>
              <b>File</b>
            </td>
            <td>
              <b>LogLevel</b>
            </td>
            <td>
              <b>Status</b>
            </td>
            <td>
              <b>End Time</b>
            </td>
            <td>
              <b>Exception</b>
            </td>
          </tr>
          <xsl:for-each select="/MigrationReport/Entries/Entry">
            <xsl:sort  select="StartTime" order="ascending"/>
            <tr>
              <td>
                <xsl:value-of select="StartTime" />
              </td>
              <td>
                <a href="{Name}" target="_blank">
                  <xsl:value-of select="Name" />
                </a>

              </td>
              <td>
                <xsl:value-of select="@Type" />
              </td>
              <td>
                <xsl:value-of select="@Status" />
              </td>
              <td>
                <xsl:value-of select="EndTime" />
              </td>
              <td>
                <xsl:value-of select="Exception" />

              </td>
            </tr>
          </xsl:for-each>

        </table>
      </body>
    </html>
  </xsl:template>

</xsl:stylesheet>


