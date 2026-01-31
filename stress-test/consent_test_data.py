import random
import uuid
import base64
import struct
from typing import Dict, List, Any, Optional


class ConsentFormTestDataGenerator:
    """Generates realistic consent form test data matching frontend structure"""

    def __init__(self, config: Dict[str, Any]):
        self.config = config
        self.payload_config = config.get('payload', {})
        
        self.signature_kb_range = self.payload_config.get('signature_kb_range', [10, 100])
        self.initials_kb_range = self.payload_config.get('initials_kb_range', [5, 30])
        self.statements_count_range = self.payload_config.get('statements_count_range', [5, 15])
        
        self.first_names = ['Emma', 'Olivia', 'Ava', 'Sophia', 'Isabella', 'Mia', 'Charlotte', 'Amelia', 'Harper', 'Evelyn']
        self.last_names = ['Smith', 'Johnson', 'Williams', 'Brown', 'Jones', 'Garcia', 'Miller', 'Davis', 'Rodriguez', 'Martinez']
        
        self.consent_statements = [
            "I understand the treatment procedure and its potential risks and benefits",
            "I have been fully informed of possible side effects and complications",
            "I consent to receive the proposed treatment(s)",
            "I understand and agree to follow all aftercare instructions provided",
            "I have disclosed all relevant medical conditions, allergies, and current medications",
            "I understand that results may vary and no specific outcome is guaranteed",
            "I consent to photography for documentation purposes (face obscured)",
            "I understand the cancellation policy and agree to notify at least 24 hours in advance",
            "I authorize payment for services rendered and understand my financial responsibilities",
            "I have had the opportunity to ask questions and all my questions have been answered to my satisfaction",
            "I understand that I can withdraw my consent at any time",
            "I confirm that I am not pregnant or nursing (if applicable)",
            "I understand that numbing agents may be used and I am not allergic to them",
            "I agree to inform the provider of any changes in my health status before each visit",
            "I understand that刮痧/拔罐 may cause temporary bruising or skin discoloration"
        ]

    def generate_new_client_consent_form(self, user_num: int = 1) -> Dict[str, Any]:
        """Generate a consent form for a new (non-existent) client"""
        return {
            "clientId": str(uuid.uuid4()),
            "printedName": self._generate_name(user_num),
            "initialedStatements": self._generate_initialed_statements(),
            "initials": self._generate_png_base64(
                self.initials_kb_range[0], 
                self.initials_kb_range[1],
                "initials"
            ),
            "signature": self._generate_png_base64(
                self.signature_kb_range[0], 
                self.signature_kb_range[1],
                "signature"
            )
        }

    def generate_robustness_payload(self, edge_case: str) -> Dict[str, Any]:
        """Generate payloads designed to test edge cases"""
        base_payload = self.generate_new_client_consent_form()
        
        if edge_case == "empty_initialed_statements":
            base_payload["initialedStatements"] = []
            
        elif edge_case == "missing_printed_name":
            del base_payload["printedName"]
            
        elif edge_case == "missing_initials":
            del base_payload["initials"]
            
        elif edge_case == "missing_signature":
            del base_payload["signature"]
            
        elif edge_case == "malformed_base64":
            base_payload["signature"] = "not-valid-base64!!!"
            base_payload["initials"] = "also-invalid"
            
        elif edge_case == "empty_client_id":
            base_payload["clientId"] = ""
            
        return base_payload

    def _generate_name(self, user_num: int = 1) -> str:
        """Generate a unique test name"""
        first = random.choice(self.first_names)
        last = random.choice(self.last_names)
        return f"{first} {last} #{user_num:04d}"

    def _generate_initialed_statements(self) -> List[str]:
        """Generate a random subset of statements"""
        num_statements = random.randint(
            self.statements_count_range[0], 
            self.statements_count_range[1]
        )
        return random.sample(self.consent_statements, num_statements)

    def _generate_png_base64(self, min_kb: int, max_kb: int, image_type: str) -> str:
        """Generate a realistic PNG-like Base64 string of specified size"""
        target_bytes = random.randint(min_kb * 1024, max_kb * 1024)
        
        png_header = b'\x89PNG\r\n\x1a\n'
        ihdr_chunk = self._create_ihdr_chunk(100, 50 if image_type == "signature" else 30)
        idat_chunk = self._create_idat_chunk(target_bytes - len(png_header) - len(ihdr_chunk) - 12)
        iend_chunk = b'\x00\x00\x00\x00IEND\xAE\x60\x82'
        
        png_data = png_header + ihdr_chunk + idat_chunk + iend_chunk
        return base64.b64encode(png_data).decode('ascii')

    def _create_ihdr_chunk(self, width: int, height: int) -> bytes:
        """Create PNG IHDR chunk"""
        data = struct.pack('>IIBBBBB', width, height, 8, 2, 0, 0, 0)
        crc = self._crc32(b'IHDR' + data)
        return struct.pack('>I', len(data)) + b'IHDR' + data + struct.pack('>I', crc)

    def _create_idat_chunk(self, target_size: int) -> bytes:
        """Create PNG IDAT chunk with compressed data"""
        raw_data = b'\x00' * target_size
        compressed = zlib.compress(raw_data, 9)
        crc = self._crc32(b'IDAT' + compressed)
        return struct.pack('>I', len(compressed)) + b'IDAT' + compressed + struct.pack('>I', crc)

    def _crc32(self, data: bytes) -> int:
        """Calculate CRC32 checksum"""
        return 0xFFFFFFFF & zlib.crc32(data)


try:
    import zlib
except ImportError:
    import sys
    print("zlib is required. Run: pip install zlib")
    sys.exit(1)
