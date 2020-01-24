# Anomymisierungs-Tool

[![Build Status](https://dev.azure.com/schulit/AnonymizationTool/_apis/build/status/SchulIT.anonymization-tool?branchName=master)](https://dev.azure.com/schulit/AnonymizationTool/_build/latest?definitionId=4&branchName=master)

Mithilfe dieses Tools lassen sich Schülerinnen und Schüler aus SchILD-NRW importieren und anschließend anonymisieren. Hierber wird ein anonymer Vor- und Nachname sowie eine anonyme E-Mail Adresse erzeugt. 

![](screenshots/overview.png)

![](screenshots/settings.png)

## Voraussetzungen

### MySQL/MariaDB und Microsoft SQL

Wird SchILD mit MySQL/MariaDB oder MSSQL betrieben, müssen keine zusätzlichen Vorraussetzungen getroffen werden. Es wird die [x64-Version](https://github.com/SchulIT/anonymization-tool/releases) empfohlen. 

### Access

Wird SchILD mit einer Access Datenbank betrieben, muss das Access Database Engine Redistributable in der entsprechenden Version vorliegen.

⚠️ **Wichtig:** ⚠️

Damit das Verbinden zu Access klappt, müssen die Architektur (x86 oder x64) von Office, der Redistributatble und von diesem Tool identisch sein. Beim 32-bit Office wird das x86 Redistributable und die [x86-Version](https://github.com/SchulIT/anonymization-tool/releases) benötigt. Analog wird das x64 Redistributable bzw. die [x64-Version](https://github.com/SchulIT/anonymization-tool/releases) benötigt.

* [Download für Office 2010](https://www.microsoft.com/de-DE/download/details.aspx?id=13255) (nicht getestet)
* [Download für Office 2013](https://www.microsoft.com/en-us/download/details.aspx?id=39358) (nicht getestet)
* [Download für Office 2016/2019/Office 365](https://www.microsoft.com/en-us/download/details.aspx?id=54920)

## Installation

Das Programm kann entweder direkt als EXE gestartet oder via MSI installiert werden. 

### Startbare EXE

Wer das Programm nicht installieren kann oder möchte, kann einfach die [aktuelle Programmversion als EXE](https://github.com/SchulIT/anonymization-tool/releases) herunterladen und per Doppelklick starten. 

**Achtung:** Das Tool ist nicht **nicht** portabel, da es Einstellungen in `C:\ProgramData\SchulIT\AnonymizationTool` ablegt.

### Installer

Einfach den [aktuellen Installer](https://github.com/SchulIT/anonymization-tool/releases) herunterladen und starten. Falls das Programm bereits installiert ist, wird es automatisch aktualisiert.

Das Programm ist in .NET Core 3 geschrieben und bringt die entsprechende Runtime direkt mit, sodass diese nicht separat installiert werden muss.

## Einrichtung

### Verbindung zu SchILD herstellen

#### Microsoft SQL

Die Verbindungszeichenfolge für die Verbindung zur SchILD-Datenbank lautet folgenermaßen:

```
Server=server\sqlexpress;Database=SchildNRW;Integrated Security=True
```

* `Server`: Hier wird der Pfad zur SQL-Server-Instanz (i.d.R. ist `server` der Computername, `sqlexpress` ist der Instanzname bei SQL Server Express)
* `Database`: Hier den gewünschten Datenbanknamen eintragen. Die Datenbank wird automatisch erstellt.
* `Integrated Security=True`: So wird der aktuelle Benutzername zur Verbindung. Alternativ lassen sich mit `User=$username%; Password=$password$` auch Benutzername und Passwort separat festlegen (anstelle von `Integrated Security=True`).

#### MySQL/MariaDB

Die Verbindungszeichenfolge für die Verbindung zur SchILD-Datenbank lautet folgenermaßen:

```
Server=localhost;Database=schildnrw;User=anonmyzation_tool_user;Password=your_secret_password;
```

* `Server`: Servername des MySQL Servers
* `Database`: Name der Datenbank auf dem MySQL Server. Die Datenbank wird automatisch angelegt, falls sie nicht vorhanden sein sollte.
* `User`: Benutzername zum Verbinden
* `Password`: Das Password des Benutzers, der sich verbinden möchte.

#### Access

Die Verbindungszeichenfolge für die Verbindung zur SchILD-Datenbank lautet folgendermaßen:

```
Driver={Microsoft Access Driver (*.mdb, *.accdb)};Dbq=$path$;Pwd=******;
```

❗ Dabei muss `$path$` durch den Pfad zur Datenbankdatei (bspw. `C:\SchILD-NRW\DB\ge_2018_19.mdb`) angepasst werden.

Das Standard-Passwort für die Access-Datenbank von SchILD muss [beim Hersteller](https://www.svws.nrw.de/) angefragt werden.

### Verbindung für die interne Datenbank

Um die anonymen Identitäten abzuspeichern, wird eine interne Datenbank verwendet. Am einfachsten ist es, eine SQLite-Datenbank zu nutzen. Wer einen SQL-Server betreibt, kann jedoch auch einen Microsoft SQL Server oder MySQL/MariaDB-Server verwenden.

Grundsätzlich wird das [Entity Framework Core](https://docs.microsoft.com/de-de/ef/core/) für die interne Datenbank verwendet. 

#### Ohne Datenbank arbeiten

Das Tool kann auch ohne persistente Datenbank arbeiten. Dazu wählt man als Art der Datenbank "SQLite" aus und nutzt folgende Verbindungszeichenfolge:

```
Datasource=:memory:
```

**Wichtig:** Anonyme Identitäten werden nicht dauerhaft gespeichert, können jedoch unmittelbar als CSV-Datei exportiert werden.

#### SQLite

```
Datasource=$path$
```

Bei $path$ den Dateinamen der SQLite-Datenbank angeben. Die Datei wird automatisch beim ersten Verbinden erstellt.

#### Microsoft SQL

```
Server=server\sqlexpress;Database=AnonymizationTool;Integrated Security=True
```

* `Server`: Hier wird der Pfad zur SQL-Server-Instanz (i.d.R. ist `server` der Computername, `sqlexpress` ist der Instanzname bei SQL Server Express)
* `Database`: Hier den gewünschten Datenbanknamen eintragen. Die Datenbank wird automatisch erstellt.
* `Integrated Security=True`: So wird der aktuelle Benutzername zur Verbindung. Alternativ lassen sich mit `User=$username%; Password=$password$` auch Benutzername und Passwort separat festlegen (anstelle von `Integrated Security=True`).

#### MySQL

```
Server=localhost;Database=anonymization_tool;User=anonmyzation_tool_user;Password=your_secret_password;
```

* `Server`: Servername des MySQL Servers
* `Database`: Name der Datenbank auf dem MySQL Server. Die Datenbank wird automatisch angelegt, falls sie nicht vorhanden sein sollte.
* `User`: Benutzername zum Verbinden
* `Password`: Das Password des Benutzers, der sich verbinden möchte.

## Verwendung

Der Workflow sieht folgendermaßen aus:

1. Benutzer aus der internen Datenbank laden
2. Benutzer aus SchILD laden
3. Anonyme Identitäten generieren

Nach Schritt 2 ist es auch möglich, Schülerinnen und Schüler, die nicht mehr in SchILD gepflegt werden, aus der internen Datenbank zu löschen. Diese Benutzer werden mit einem roten "x" markiert. 

**Wichtig:** Beim Löschen werden diese Benutzer jedoch nicht entgültig gelöscht, sondern nur als gelöscht markiert. Ein Wiederherstellen auf Datenbankebene ist somit möglich.

## Export

Anonyme Identitäten können als CSV-Datei exportiert werden, um sie dann weiterzuverarbeiten. 

## Upgrade

Das Aktualisieren des Tools ist unkompliziert. Bei der MSI-Variante muss einfach die neueste MSI installiert werden. Nutzt man die startbare EXE-Datei, kann diese einfach ersetzt werden. 

## Probleme?

Dann bitte in den [Issue schauen](https://github.com/SchulIT/anonymization-tool/issues), ob das Problem bereits bekannt ist. Falls nicht, kann dort ein Issue geöffnet werden. Support via E-Mail wird nicht angeboten.

## Projekt selber bauen

Eine kurze Anleitung zum Bauen des Projektes gibt es [hier](BUILD.md).

## Lizenz

[MIT License](LICENSE.md)

## Mitmachen

Wer möchte, darf gerne Bugs melden oder Pull Requests einreichen :smile: