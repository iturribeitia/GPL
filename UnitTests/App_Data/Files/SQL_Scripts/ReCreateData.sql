--select OBJECT_ID('[SampleTable]');

if OBJECT_ID('[SampleTable]') is not null 
 drop table  [SampleTable];

CREATE TABLE [SampleTable] (
  [Coll1] VARCHAR(30) NOT NULL,
  [Coll,2] VARCHAR(30) NOT NULL,
  [Coll"3] VARCHAR(30) NOT NULL,
  [Coll'4] VARCHAR(30) NOT NULL
  
);

INSERT INTO [SampleTable] 
    ([Coll1], [Coll,2], [Coll"3], [Coll'4]) 
VALUES 
    ('R1 Val 1','R1 Val 2','R1 Val 3','R1 Val 4'),
    ('R2, Val 1','R2, Val 2','R2, Val 3','R2, Val 4'),
    ('R3" Val 1','R3" Val 2','R3" Val 3','R3" Val 4'),
    ('R4'' Val 1','R4'' Val 2','R4'' Val 3','R4'' Val 4');

select [Coll1], [Coll,2], [Coll"3], [Coll'4] from [SampleTable] ;

