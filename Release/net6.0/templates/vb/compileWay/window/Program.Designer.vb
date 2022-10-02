<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Renderer
    Inherits System.Windows.Forms.Form

    'Form remplace la méthode Dispose pour nettoyer la liste des composants.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Requise par le Concepteur Windows Form
    Private components As System.ComponentModel.IContainer

    'REMARQUE : la procédure suivante est requise par le Concepteur Windows Form
    'Elle peut être modifiée à l'aide du Concepteur Windows Form.  
    'Ne la modifiez pas à l'aide de l'éditeur de code.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Renderer))
        Me.frameRefreshTimer = New System.Windows.Forms.Timer(Me.components)
        Me.display = New System.Windows.Forms.PictureBox()
        CType(Me.display, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'frameRefreshTimer
        '
        Me.frameRefreshTimer.Interval = 33
        '
        'display
        '
        Me.display.Dock = System.Windows.Forms.DockStyle.Fill
        Me.display.Location = New System.Drawing.Point(0, 0)
        Me.display.Name = "display"
        Me.display.Size = New System.Drawing.Size(484, 461)
        Me.display.TabIndex = 0
        Me.display.TabStop = False
        '
        'Renderer
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(484, 461)
        Me.Controls.Add(Me.display)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "Renderer"
        Me.Text = "Application"
        CType(Me.display, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents frameRefreshTimer As Timer
    Friend WithEvents display As PictureBox
End Class
