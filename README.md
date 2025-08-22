# 🌍 Global Dictionary

Global Dictionary is a service for storing and managing static values in multiple languages.  
It is designed to provide a centralized solution for multilingual content management, allowing applications to remain flexible, consistent, and easy to maintain.

---

## ✨ Core Concepts

### 🔑 Values
Values represent individual static entries (such as messages, labels, or UI strings).  
Each value can exist in multiple languages, making it possible to request the same content in the desired language.  
If a language is not supported, fallback mechanisms can be implemented depending on the client’s needs.

Key capabilities:
- Create and manage static values in different languages.
- Retrieve localized content for a specific language or across all available languages.
- Ensure consistent usage of static text across platforms.

---

### 📝 Templates
Templates provide a way to group values together.  
They act as reusable collections of related entries that can be requested as a whole.

With templates you can:
- Load predefined sets of values into a template.
- Request all values of a template in a chosen language (or in all languages).
- Add or remove values from templates dynamically.

This feature ensures modularity and simplifies managing groups of related content.

---

### 🗂️ Categories
Categories serve as a hierarchical tree of objects used for navigation and organization.  
Unlike templates and values, categories form a separate structural system.

Categories allow you to:
- Create nested structures with unlimited depth.
- Assign custom types to categories for better semantic organization.
- Change order of categories within the same level.
- Add, edit, or remove categories as needed.

This system provides a flexible way to structure content and improve navigation across different layers of data.

---

## 🚀 Purpose
Global Dictionary was created to solve the challenge of managing multilingual static data in modern applications.  
Instead of scattering translations and static texts across different systems, it offers a **single source of truth**, ensuring that content is always consistent, reusable, and easy to maintain.

By combining **Values**, **Templates**, and **Categories**, Global Dictionary provides a complete toolkit for handling localized data and organizing it in a structured, developer-friendly way.

---

## 📖 Documentation
Full API and usage documentation is available at: /docs 


---

## 📌 Status
The project is under active development.  
Future updates will expand functionality and improve developer experience.
