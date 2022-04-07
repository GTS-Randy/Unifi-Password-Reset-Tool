Imports MongoDB.Driver
Imports MongoDB.Bson
Imports System.Text
Imports System.Security.Cryptography
Imports CryptSharp

Public Class frm_Main

    Const HashedPassword = "$6$ybLXKYjTNj9vv$dgGRjoXYFkw33OFZtBsp1flbCpoFQR7ac8O0FrZixHG.sw2AQmA5PuUbQC/e5.Zu.f7pGuF7qBKAfT/JRZFk8/"
    Private server As New MongoServerAddress("Localhost", 27117)
    Private dbsettings As New MongoClientSettings
    Private client As MongoClient
    Private crypto As New CryptSystem

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load
        ' Hased Password: 
        ' "password" = "$6$ybLXKYjTNj9vv$dgGRjoXYFkw33OFZtBsp1flbCpoFQR7ac8O0FrZixHG.sw2AQmA5PuUbQC/e5.Zu.f7pGuF7qBKAfT/JRZFk8/"
        lstAccounts.Items.Clear()
        For Each acct As UserAccount In Read_DB()
            lstAccounts.Items.Add(acct)
        Next
    End Sub

    Private Function Read_DB() As List(Of UserAccount)
        dbsettings.Server = server
        dbsettings.DirectConnection = True
        client = New MongoClient(dbsettings)
        Dim db As MongoDatabaseBase = client.GetDatabase("ace")
        Dim col = db.ListCollectionNames()
        Dim Collection As IMongoCollection(Of BsonDocument) = db.GetCollection(Of BsonDocument)("admin")
        Dim q = New BsonDocument
        Dim f = Builders(Of BsonDocument).Projection.Include("_id")
        Dim accts As List(Of BsonDocument) = Collection.Find(q).ToList
        Dim AdminAccounts As New List(Of UserAccount)
        For Each acct As BsonDocument In accts
            'If acct.Item("name") = "Admin" Or acct.Item("name") = "admin" Then
            'Return New UserAccount(acct.Item("_id").ToString, acct.Item("name").ToString, acct.Item("email").ToString, acct.Item("x_shadow").ToString, acct.Item("time_created").ToString, acct.Item("last_site_name").ToString)
            'End If
            AdminAccounts.Add(New UserAccount(acct.Item("_id").ToString, acct.Item("name").ToString, acct.Item("email").ToString, acct.Item("x_shadow").ToString, acct.Item("time_created").ToString, acct.Item("last_site_name").ToString))
        Next

        Return AdminAccounts
    End Function

    Private Sub Update_Password()
        dbsettings.Server = server
        dbsettings.DirectConnection = True
        client = New MongoClient(dbsettings)
        Dim db As MongoDatabaseBase = client.GetDatabase("ace")
        Dim col = db.ListCollectionNames()
        Dim userCollection As IMongoCollection(Of BsonDocument) = db.GetCollection(Of BsonDocument)("admin")
        Dim q = New BsonDocument
        Dim accts As List(Of BsonDocument) = userCollection.Find(q).ToList
        Dim filterByname = Builders(Of BsonDocument).Filter.Eq(Of String)("name", "Admin")
        'userCollection.UpdateOne(filterByname, New BsonDocument("$set", New BsonDocument("password", HashedPassword)))
        'For Each acct As BsonDocument In accts
        userCollection.UpdateOne(Builders(Of BsonDocument).Filter.Eq(Of String)("name", lstAccounts.SelectedItem.UserName), New BsonDocument("$set", New BsonDocument("x_shadow", HashedPassword)))
        MsgBox($"Password for { lstAccounts.SelectedItem.UserName } set to `password`", MsgBoxStyle.OkOnly, "Password Reset")
        'Next
    End Sub


    Private Sub lstAccounts_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstAccounts.SelectedIndexChanged
        Dim UsrAccount As UserAccount = lstAccounts.SelectedItem
        txtEmail.Text = UsrAccount.Email
        txtUsername.Text = UsrAccount.UserName
        txtPassword.Text = UsrAccount.Password
    End Sub

    Private Sub txtPassword_DoubleClick(sender As Object, e As EventArgs) Handles txtPassword.DoubleClick
        txtPassword.ReadOnly = False
    End Sub

    Private Sub txtPassword_KeyDown(sender As Object, e As KeyEventArgs) Handles txtPassword.KeyDown
        If e.KeyCode = Keys.Enter Then txtPassword.ReadOnly = True
    End Sub

    Private Sub btn_Reset_Click(sender As Object, e As EventArgs) Handles btn_Reset.Click
        Update_Password()
    End Sub
End Class

Public Class UserAccount

    Private _ID As String
    Private _Name As String
    Private _Email As String
    Private _Password As String
    Private _CreatedTime As String
    Private _LastSite As String

    Public ReadOnly Property ID As String
        Get
            Return _ID
        End Get
    End Property
    Public ReadOnly Property UserName As String
        Get
            Return _Name
        End Get
    End Property
    Public ReadOnly Property Email As String
        Get
            Return _Email
        End Get
    End Property
    Public ReadOnly Property Password As String
        Get
            Return _Password
        End Get
    End Property
    Public ReadOnly Property Last_Site As String
        Get
            Return _LastSite
        End Get
    End Property

    Sub New()

    End Sub
    Sub New(Name As String, Email As String)
        _Name = Name
        _Email = Email
    End Sub
    Sub New(Name As String, Email As String, Password As String)
        _Name = Name
        _Email = Email
        _Password = Password
    End Sub
    Sub New(ID As String, Name As String, Email As String, Password As String)
        _ID = ID
        _Name = Name
        _Email = Email
        _Password = Password
    End Sub
    Sub New(ID As String, Name As String, Email As String, Password As String, Time_Created As String, Last_Site As String)
        _ID = ID
        _Name = Name
        _Email = Email
        _Password = Password
        _CreatedTime = Time_Created
        _LastSite = Last_Site
    End Sub

    Public Overrides Function ToString() As String
        Return _Name
    End Function
End Class

Class CryptSystem
    ' quickhash.com
    ' SHA-512 / crypt(3) / $6$
    Private Const OLDSALT As String = "9Ter1EZ9$lSt6"
    Private Const NEWSALT As String = "4NfmBk8A"

    Dim test As SHA512

    Sub New()

    End Sub

    Public Function computeNewHash(ByVal clearText As String) As Byte()

        Dim encoder As New Text.UTF8Encoding()
        Dim sha512hasher As New System.Security.Cryptography.SHA512Managed()
        Return sha512hasher.ComputeHash(encoder.GetBytes(clearText & NEWSALT))

    End Function

    Public Function GenerateNewSaltedHash(plainText As String) As Byte()
        Dim algorithm As HashAlgorithm = New SHA256Managed()
        Dim plainTextWithSaltBytes As Byte() = Encoding.ASCII.GetBytes(plainText & NEWSALT)

        Return algorithm.ComputeHash(plainTextWithSaltBytes)
    End Function

    Public Function Generate_SHA512_Hash(ClearText As String, Optional salt As String = "abc") As String
        'Dim SHACrypter As ShaCrypter
        'Dim Sha512Crypter As Sha512Crypter


        'ShaCrypter.Crypt("Linux8902*")
        'Sha512Crypter.GetCrypter("$6$4NfmBk8A$S.qWAuFYSZ2aPyNesZS8Wq97fNt8H8NR.cmUH7rZ6qeC7KWU3PRuAJsrlnrrvZU6z5K3xdbo0O2yIMYqIciX40")
        'Sha512Crypter.Crypt("Linux8902*")
        Dim sha512C As Sha512Crypter = New Sha512Crypter
        Dim opt As New CrypterOptions

        'Dim test = sha512C.Crypt(ClearText, salt)

        Return sha512C.Crypt(ClearText, Crypter.Blowfish.GenerateSalt())
    End Function




End Class

Class RNGCSP
    Private Shared rngCsp As New RNGCryptoServiceProvider()
    ' Main method.
    Public Shared Sub Main()
        Const totalRolls As Integer = 25000
        Dim results(5) As Integer

        ' Roll the dice 25000 times and display
        ' the results to the console.
        Dim x As Integer
        For x = 0 To totalRolls
            Dim roll As Byte = RollDice(System.Convert.ToByte(results.Length))
            results((roll - 1)) += 1
        Next x
        Dim i As Integer

        While i < results.Length
            Console.WriteLine("{0}: {1} ({2:p1})", i + 1, results(i), System.Convert.ToDouble(results(i)) / System.Convert.ToDouble(totalRolls))
            i += 1
        End While
        rngCsp.Dispose()
    End Sub


    ' This method simulates a roll of the dice. The input parameter is the
    ' number of sides of the dice.
    Public Shared Function RollDice(ByVal numberSides As Byte) As Byte
        If numberSides <= 0 Then
            Throw New ArgumentOutOfRangeException("NumSides")
        End If
        ' Create a byte array to hold the random value.
        Dim randomNumber(0) As Byte
        Do
            ' Fill the array with a random value.
            rngCsp.GetBytes(randomNumber)
        Loop While Not IsFairRoll(randomNumber(0), numberSides)
        ' Return the random number mod the number
        ' of sides.  The possible values are zero-
        ' based, so we add one.
        Return System.Convert.ToByte(randomNumber(0) Mod numberSides + 1)

    End Function


    Private Shared Function IsFairRoll(ByVal roll As Byte, ByVal numSides As Byte) As Boolean
        ' There are MaxValue / numSides full sets of numbers that can come up
        ' in a single byte.  For instance, if we have a 6 sided die, there are
        ' 42 full sets of 1-6 that come up.  The 43rd set is incomplete.
        Dim fullSetsOfValues As Integer = [Byte].MaxValue / numSides

        ' If the roll is within this range of fair values, then we let it continue.
        ' In the 6 sided die case, a roll between 0 and 251 is allowed.  (We use
        ' < rather than <= since the = portion allows through an extra 0 value).
        ' 252 through 255 would provide an extra 0, 1, 2, 3 so they are not fair
        ' to use.
        Return roll < numSides * fullSetsOfValues

    End Function 'IsFairRoll
End Class