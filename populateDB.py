import sqlite3
import pandas as pd

df = pd.read_csv("data.csv")

df.columns = df.columns.str.strip()

connection = sqlite3.connect("db1.db")

df.to_sql("Appointments", connection, if_exists="append", index=False)

connection.close()
