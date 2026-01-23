import random
import uuid
import base64
from datetime import datetime, timedelta
from typing import Dict, List, Any, Optional


class TestDataGenerator:
    """Generates realistic test data for the Kimmy Esthi API stress tests"""
    
    def __init__(self, config: Dict[str, Any]):
        self.config = config
        self.services = config.get('test_data', {}).get('services', [])
        self.skin_concerns = config.get('test_data', {}).get('skin_concerns', [])
        self.date_range = config.get('test_data', {}).get('appointment_date_range', {})
        
        # Sample names and domains for email generation
        self.first_names = ['Emma', 'Olivia', 'Ava', 'Sophia', 'Isabella', 'Mia', 'Charlotte', 'Amelia', 'Harper', 'Evelyn']
        self.last_names = ['Smith', 'Johnson', 'Williams', 'Brown', 'Jones', 'Garcia', 'Miller', 'Davis', 'Rodriguez', 'Martinez']
        self.email_domains = ['gmail.com', 'yahoo.com', 'hotmail.com', 'outlook.com', 'icloud.com']
        
        # Sample appointment IDs (these should be available appointments in the database)
        self.appointment_ids = [
            str(uuid.uuid4()) for _ in range(100)
        ]
        
        # Store real appointment IDs fetched from API
        self.available_appointment_ids: List[str] = []
        
    def generate_appointment_request(self, appointment_id: Optional[str] = None) -> Dict[str, Any]:
        """Generate a realistic appointment booking request matching frontend structure"""
        if appointment_id is None:
            appointment_id = random.choice(self.appointment_ids)
            
        return {
            "appointmentId": appointment_id,
            "scheduledAppointment": {
                "serviceName": random.choice(self.services),
                "client": {
                    "preferredName": self._generate_name(),
                    "email": self._generate_email(),
                    "phoneNumber": self._generate_phone_number()
                },
                "skinConcerns": random.choice(self.skin_concerns)
            },
            "promotion": self._generate_promotion() if random.random() < 0.3 else None
        }
    
    def generate_consent_form(self, client_id: str = "") -> Dict[str, Any]:
        """Generate a realistic consent form matching frontend structure"""
        if not client_id:
            client_id = str(uuid.uuid4())
            
        return {
            "clientId": client_id,
            "printedName": self._generate_name(),
            "initialedStatements": self._generate_initialed_statements(),
            "initials": self._generate_base64_initials(),
            "signature": self._generate_base64_signature()
        }
    
    def generate_login_request(self, username: str, password: str) -> Dict[str, str]:
        """Generate admin login request"""
        return {
            "Username": username,
            "Password": password
        }
    
    def _generate_name(self) -> str:
        """Generate a random person's name"""
        return f"{random.choice(self.first_names)} {random.choice(self.last_names)}"
    
    def _generate_email(self) -> str:
        """Generate a unique email address"""
        first_name = random.choice(self.first_names).lower()
        last_name = random.choice(self.last_names).lower()
        number = random.randint(1, 9999)
        domain = random.choice(self.email_domains)
        return f"{first_name}.{last_name}{number}@{domain}"
    
    def _generate_phone_number(self) -> str:
        """Generate a realistic US phone number"""
        area_codes = ['555', '212', '646', '917', '718', '347', '929', '516', '631', '914']
        exchange = random.randint(200, 999)
        number = random.randint(1000, 9999)
        return f"{random.choice(area_codes)}-{exchange}-{number}"
    
    def _generate_initialed_statements(self) -> List[str]:
        """Generate typical consent form statements"""
        statements = [
            "I understand the treatment procedure",
            "I have been informed of potential risks", 
            "I consent to the treatment",
            "I understand aftercare instructions",
            "I have disclosed relevant medical conditions",
            "I understand payment terms",
            "I consent to photography if needed",
            "I understand cancellation policy"
        ]
        # Return a random subset
        num_statements = random.randint(5, len(statements))
        return random.sample(statements, num_statements)
    
    def _generate_initials(self) -> str:
        """Generate realistic initials (2-3 letters)"""
        first_name = random.choice(self.first_names)[0]
        last_name = random.choice(self.last_names)[0]
        middle_initial = random.choice(['', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'L', 'M'])
        return f"{first_name}{middle_initial}{last_name}".strip()
    
    def _generate_signature(self) -> str:
        """Generate a realistic signature representation"""
        return f"Digitally signed by {self._generate_name()} on {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}"
    
    def _generate_base64_initials(self) -> str:
        """Generate a simple Base64 representation of initials"""
        initials = self._generate_initials()
        # Create a simple text-based signature and encode to Base64
        signature_text = f"Initials: {initials}"
        return base64.urlsafe_b64encode(signature_text.encode()).decode()
    
    def _generate_base64_signature(self) -> str:
        """Generate a simple Base64 representation of signature"""
        name = self._generate_name()
        timestamp = datetime.now().strftime('%Y-%m-%d %H:%M:%S')
        # Create a simple text-based signature and encode to Base64
        signature_text = f"Signed: {name} on {timestamp}"
        return base64.urlsafe_b64encode(signature_text.encode()).decode()
    
    def _generate_promotion(self) -> Optional[Dict[str, Any]]:
        """Generate a promotion object"""
        # Only include promotion if random chance, and make it optional to match manual test
        if random.random() < 0.3:  # 30% chance
            return {
                "name": random.choice([
                    "New Client Special",
                    "Holiday Promotion", 
                    "Summer Glow Package",
                    "Birthday Discount",
                    "Referral Reward",
                    "Flash Sale"
                ])
            }
        else:
            return None  # No promotion (matches working manual test)
    
    def get_available_dates(self) -> List[str]:
        """Get list of available appointment dates for testing GET endpoints"""
        start_date = datetime.strptime(self.date_range.get('start_date', '2024-01-01'), '%Y-%m-%d')
        end_date = datetime.strptime(self.date_range.get('end_date', '2024-12-31'), '%Y-%m-%d')
        
        dates = []
        current_date = start_date
        while current_date <= end_date:
            dates.append(current_date.strftime('%Y-%m-%d'))
            current_date += timedelta(days=1)
            
        return dates