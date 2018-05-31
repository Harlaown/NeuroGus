# NeuroGus
░░░░░▄▀▀▀▄░  
▄███▀░◐░░░▌░░░░░░░   
░░░░▌░░░░░▐░░░░░░░   
░░░░▐░░░░░▐░░░░░░░   
░░░░▌░░░░░▐▄▄░░░░░   
░░░░▌░░░░▄▀▒▒▀▀▀▀▄   
░░░▐░░░░▐▒▒▒▒▒▒▒▒▀▀▄   
░░░▐░░░░▐▄▒▒▒▒▒▒▒▒▒▒▀▄   
░░░░▀▄░░░░▀▄▒▒▒▒▒▒▒▒▒▒▀▄   
░░░░░░▀▄▄▄▄▄█▄▄▄▄▄▄▄▄▄▄▄▀▄   
░░░░░░░░░░░▌▌░▌▌░░░░░   
░░░░░░░░░░░▌▌░▌▌░░░░░   
░░░░░░░░░▄▄▌▌▄▌▌░░░░░  


### Для запуска потребуется:
- .NET Framework версии >= 4.7
- Sql server(Любой) + Sql Server localdb

### Для работы так же потребуется датасет с уже классифицироваными данными в виде Excel файла следующего формата:

| Текст Обращения | Классификация 1 | Классификация 2 | ... | Классификация N |
|-----------------|-----------------|-----------------|----:|-----------------|
| Текст           | Вариант 1       | Вариант 2       |     | Вариант 2       |
| Текст           | Вариант 1       | Вариант 1       |     | Вариант 3       |
| ...             | ...             | ...             |     | ...             |

*В проекте уже лежит пример датасета, но качество и размер его оставляет желать лучшего*

Имя: testdataset.xlsx

Директория: ../NeuroGus.Wpf/bin/debug/Files/

При замене рассположение и имя оставить тем же


### Запуск
- Запустить .../NeuroGus.Wpf/bin/Debug/NeuroGus.Wpf.exe
- Дождаться окончания процесса обучения
- ????
- Вводить запрос в строку и нажимать Enter




