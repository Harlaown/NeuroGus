using System.Collections.Generic;
using System.IO;
using System.Linq;
using NeuroGus.Core.Model;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace NeuroGus.Core.Parser
{
    public static class ExcelParser
    {
        public static List<ClassifiableText> XlsxToClassifiableTexts(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var excelFile = new XSSFWorkbook(fs);

                var sheet = excelFile.GetSheetAt(0);
                if (sheet.LastRowNum <= 0) return null;
                var characteristics = GetCharacteristics(sheet);
                var classifiableTexts = new List<ClassifiableText>();

                // start from second row
                for (var i = 1; i <= sheet.LastRowNum; i++)
                {
                    var characteristicsValues = GetCharacteristicsValues(sheet.GetRow(i), characteristics);

                    // exclude empty rows
                    if (sheet.GetRow(i).GetCell(0).StringCellValue.Equals("")) continue;
                    if (characteristicsValues == null) continue;
                    classifiableTexts.Add(new ClassifiableText(sheet.GetRow(i).GetCell(0).StringCellValue,
                        characteristicsValues));
                }

                return classifiableTexts;
            }
        }

        private static ICollection<KeyValuePair> GetCharacteristicsValues(IRow row,
            IReadOnlyList<Characteristic> characteristics)
        {
            if (row == null)
                return null;
            var characteristicsValues = new List<KeyValuePair>();


            for (var i = 1; i < row.LastCellNum; i++)
            {
                var newchar = new CharacteristicValue(row.GetCell(i).StringCellValue);
                characteristics[i - 1].PossibleValues = new List<CharacteristicValue> {newchar};

                characteristicsValues.Add(new KeyValuePair
                {
                    Key = characteristics[i - 1],
                    Value = characteristics[i - 1].PossibleValues.FirstOrDefault()
                });
            }

            return characteristicsValues;
        }

        private static List<Characteristic> GetCharacteristics(ISheet sheet)
        {
            var characteristics = new List<Characteristic>();

            // first row from second to last columns contains Characteristics names
            for (var i = 1; i < sheet.GetRow(0).LastCellNum; i++) characteristics.Add(new Characteristic(sheet.GetRow(0).GetCell(i).StringCellValue));

            return characteristics;
        }
    }
}
