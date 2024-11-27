import sqlite3
import pandas as pd

df = pd.read_csv('data.csv')

df.columns = df.columns.str.strip()

connection = sqlite3.connect('db1.db')

df.to_sql('nonbooked_appointments', connection, if_exists='replace')

connection.close()
