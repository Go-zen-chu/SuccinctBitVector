SuccinctBitVector
=================

SuccinctBitVector class written in C#.

To write this code I learned most of the things about SuccinctBitVector from [this site](http://d.hatena.ne.jp/echizen_tm/20110811/1313083180).

Thank you very much.

usage:
(in case of int)
var sbv = new SuccinctBitVector<int>();
sbv.SetValue(20, 1);
sbv.SetValue(4000, 2);
sbv.SetValue(15000, 3);
// You should build sbv after you put all of your data
sbv.Build();

sbv.GetValue_Numeric(0); // => 0
sbv.GetValue_Numeric(20); // => 1
sbv.GetValue_Numeric(15000); // => 3

(in case of string or other nullable classes)
var sbv = new SuccinctBitVector<int>();
sbv.SetValue(20, "1");
sbv.SetValue(4000, "2");
sbv.SetValue(15000, "3");
// You should build sbv after you put all of your data
sbv.Build();

// You should use GetValue_Nullable otherwise you'll get exception
sbv.GetValue_Nullable(0); // => null
sbv.GetValue_Nullable(20); // => "1"
sbv.GetValue_Nullable(15000); // => "3"
