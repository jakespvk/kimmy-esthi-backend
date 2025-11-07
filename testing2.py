import datetime
import csv
import uuid

# Number of days to generate appointments for
NUM_DAYS = 26

data = []

start_hour = 9  # 9 AM
end_hour = 21  # 9 PM (in 24-hour format)

for day_offset in range(NUM_DAYS):
    base_date = datetime.datetime.now().date() + datetime.timedelta(days=day_offset)
    for hour in range(start_hour, end_hour + 1):  # inclusive of 9 PM
        appt_id = uuid.uuid4()
        appt_datetime = datetime.datetime.combine(base_date, datetime.time(hour))
        appt_status = "Available"
        data.append({"id": appt_id, "datetime": appt_datetime, "status": appt_status})

file = "data.csv"
with open(file, "w", newline="") as f:
    fieldnames = ["id", "datetime", "status"]
    writer = csv.DictWriter(f, fieldnames=fieldnames)
    writer.writeheader()
    writer.writerows(data)
