**Перед першим запуском `vagrant up` обов'язково виконайте ці кроки:**

1.  **Встановіть ПЗ:**
    * [VirtualBox](https://www.virtualbox.org/wiki/Downloads)
    * [Vagrant](https://www.vagrantup.com/downloads)

2.  **Вимкніть конфліктні функції Windows:**
    * Натисніть "Пуск" і напишіть "**Turn Windows features on or off**" ("Увімкнення або вимкнення компонентів Windows").
    * У вікні, **зніміть галочки** з:
        * `Virtual Machine Platform` (Платформа віртуальної машини)
        * `Windows Hypervisor Platform` (Платформа гіпервізора Windows)
        * `Підсистема Windows для Linux` (Якщо увімкнена)
        * `Hyper-V` (Якщо увімкнений)

3.  **Перезавантажте ПК** 

## Запуск проєкту

Після виконання вимог вище, просто запустіть ці команди:

```bash
# 1. Склонуйте/ спульте код з репо

# 2. vagrant destroy -f - видаліть старі VM

# 3. vagrant up - запуск VM-ок і деплою

Сервер Ubuntu: http://localhost:15001/swagger/index.html

Сервер CentOS: http://localhost:15002/swagger/index.html