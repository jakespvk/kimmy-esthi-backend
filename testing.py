import datetime
import csv

data = []

for i in range(13):
    if (i + 9 == 12):
        apptTime = f"{i + 9}:00 PM"
    elif (i + 9 > 12):
        apptTime = f"{i + 9 - 12}:00 PM"
    else:
        apptTime = f"{i + 9}:00 AM"
    for j in range(26):
        apptId = i + (13 * j)
        print(apptId)
        apptDate = (datetime.datetime.now() + datetime.timedelta(days=j)).strftime("%m/%d/%Y")
        apptStatus = "Available"
        data.append({
                        "id": apptId,
                        "date": apptDate,
                        "time": apptTime,
                        "status": apptStatus
                    })

        

file = "data.csv"
with open(file, 'w', newline='') as f:
    fieldnames = ["id", "date", "time", "status"]
    writer = csv.DictWriter(f, fieldnames=fieldnames)
    writer.writeheader()
    writer.writerows(data)
