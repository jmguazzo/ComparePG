# ComparePG - PostgreSQL schema comparison in C# - [![Build Status](https://travis-ci.org/ncareau/ComparePG.svg?branch=master)](https://travis-ci.org/ncareau/ComparePG)

This tool compares two PostgreSQL database and generate the script to adapt the target database.

It is based on the excellent [pgdiff](https://github.com/joncrlsn/pgdiff) from [Jon Carlson](https://github.com/joncrlsn) but, as pgdiff is in GO, it doesn't fit nicely in Microsoft oriented places.

Based on this tool, I kept the same order for analyzing diffs between databases.
I added analysis for comments.

The order of analysis is :
1. Group roles
1. Schema
1. Sequence
1. Table
1. Column
1. Index
1. View
1. Comment
1. Functions
1. Foreign key
1. Owner
1. Grant
1. Trigger

This tool doesn't analyse LOGIN roles because it's impossible to script the password.
If you still need it, simply comment the dedicated section in the GroupRole class.
Due to the way it's processed, it may need multiple runs to completely prepare the migration script.

PostgreSQL being what it is, dependencies between views are enforced and if you modify a view that's the source of another view, you will have to drop and recreate both view in the good order.
As of now, ComparePG can find dependencies on 1 level and warns you when it find it with the message
```
--!!!! ATTENTION This view is used by other views !
--[List of views]
```

### Download
An [executable](executable.zip) version is available.

### Usage


```
ComparePG.exe {parameter}=[value]... [Items]
SOURCE :
       SRVR_S : server
       BASE_S : database
       PORT_S : port
       SCHM_S : schema
       LGN_S : login
       PWD_S : password
TARGET :
       SRVR_T : server
       BASE_T : database
       PORT_T : port
       SCHM_T : schema
       LGN_T : login
       PWD_T : password
Items
Any of the caracter representing an item comparison. If none given ALL will be assumed.
        g : group roles
        s : schema
        S : sequence
        t : table
        c : column
        i : index
        v : view
        C : comment
        f : functions
        F : foreign key
        o : owner
        G : grant
        T : trigger
ex :
ComparePG.exe SRVR_S=localhost BASE_S=bd1 PORT_S=5432 SCHM_S=public LGN_S=user1 PWD_S=***** SRVR_T=server BASE_T=bd1 PORT_T=5433 SCHM_T=public LGN_T=user1 PWD_T=*****
```



