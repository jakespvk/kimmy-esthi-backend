import datetime
import csv
import uuid

data = []

for i in range(13):
    if i + 9 == 12:
        apptTime = f"{i + 9}:00 PM"
    elif i + 9 > 12:
        apptTime = f"{i + 9 - 12}:00 PM"
    else:
        apptTime = f"{i + 9}:00 AM"
    for j in range(26):
        apptId = str(uuid.uuid4())
        print(apptId)
        apptDate = (datetime.datetime.now() + datetime.timedelta(days=j)).strftime(
            "%m/%d/%Y"
        )
        (apptMonth, apptDay, apptYear) = apptDate.split("/")
        apptMonth = int(apptMonth)
        apptDay = int(apptDay)
        apptDate = f"{apptMonth}/{apptDay}/{apptYear}"
        apptStatus = "Available"
        data.append(
            {"id": apptId, "date": apptDate, "time": apptTime, "status": apptStatus}
        )


file = "data.csv"
with open(file, "w", newline="") as f:
    fieldnames = ["id", "date", "time", "status"]
    writer = csv.DictWriter(f, fieldnames=fieldnames)
    writer.writeheader()
    writer.writerows(data)
