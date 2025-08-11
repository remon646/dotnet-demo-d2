# Demo Data Specifications

## Demo Data Overview

This document defines the demo data structure and content used in the Employee Information Management Mock Application for demonstration and testing purposes.

## Employee Data

### Initial Demo Employees
- **Count**: 10 initial demo employees (EMP2024001-EMP2024010)
- **Names**: Diverse Japanese names for realistic demonstration
  - 山田太郎 (Yamada Taro)
  - 佐藤花子 (Sato Hanako)
  - 田中一郎 (Tanaka Ichiro)
  - 鈴木美咲 (Suzuki Misaki)
  - 高橋健太 (Takahashi Kenta)
  - 伊藤由美 (Ito Yumi)
  - 渡辺直樹 (Watanabe Naoki)
  - 小林恵子 (Kobayashi Keiko)
  - 加藤雅彦 (Kato Masahiko)
  - 吉田真理 (Yoshida Mari)

### Employee Distribution
- **Department Balance**: Evenly distributed across all departments
- **Position Balance**: Realistic hierarchy distribution across all positions
- **Join Date Variety**: Varied join dates spanning 2022-2024 for realistic data distribution
- **Contact Information**: Realistic email addresses and phone numbers

### Employee ID Format
- **Format**: EMPYYYYNNN (e.g., EMP2024001)
- **Components**:
  - **EMP**: Fixed prefix for all employees
  - **YYYY**: Join year (4 digits, e.g., 2024)
  - **NNN**: Sequential number (001-999)
- **Generation**: Automatic generation with thread-safe reservation system
- **Validation**: Format validation on all employee operations

## Department Data

### Available Departments
1. **Sales Department (営業部)**
   - Department Code: SALES
   - Focus: Customer relations and business development
   
2. **Development Department (開発部)**
   - Department Code: DEV
   - Focus: Software development and technical innovation
   
3. **General Affairs Department (総務部)**
   - Department Code: GA
   - Focus: Administrative operations and support
   
4. **Human Resources Department (人事部)**
   - Department Code: HR
   - Focus: Personnel management and organizational development

## Position Hierarchy

### Position Levels
1. **Department Manager (部長)**
   - Highest level management
   - Strategic decision making
   
2. **Section Manager (課長)**
   - Middle management
   - Team leadership and coordination
   
3. **Supervisor (主任)**
   - Team lead level
   - Direct supervision and guidance
   
4. **General (一般)**
   - Individual contributor
   - Operational execution

## Demo User Accounts

### Administrative Access
- **Username**: admin
- **Password**: password
- **Permissions**: Full access to all features and data

### Regular User Access
- **Username**: user
- **Password**: password
- **Permissions**: Standard user access (future role-based restrictions)

## Data Relationships

### Employee-Department Mapping
- Each employee is assigned to exactly one department
- Department distribution ensures realistic organizational structure
- Department managers are appropriately assigned within their departments

### Position Distribution
- Realistic hierarchy with appropriate ratios
- Multiple employees per department with varied positions
- Management positions balanced across departments

## Data Validation Rules

### Employee Data Validation
- **Name**: Required, maximum 50 characters
- **Employee Number**: Auto-generated, unique, format EMPYYYYNNN
- **Department**: Required, must exist in department master
- **Position**: Required, must be valid position
- **Join Date**: Required, date format YYYY/MM/DD
- **Email**: Required, valid email format, maximum 100 characters
- **Phone**: Optional, maximum 15 characters

### Department Data Validation
- **Department Code**: Required, unique identifier
- **Department Name**: Required, descriptive name
- **Active Status**: Boolean flag for department availability

## Future Data Extensions

### Planned Enhancements
- **Profile Images**: Employee photo uploads
- **Additional Contact Info**: Emergency contacts, addresses
- **Organizational Chart**: Hierarchical reporting relationships
- **Department Budgets**: Financial information per department
- **Performance Data**: Employee evaluations and metrics