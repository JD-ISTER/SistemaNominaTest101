import customtkinter as ctk
from tkinter import filedialog, messagebox
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg
#importar modelos (clases manuales de Regresión Lineal y KNN)
from modelos import RegresionLinealManual, KNNManual

#configuracion de apariencia de la aplicacion
ctk.set_appearance_mode("dark")
ctk.set_default_color_theme("green")

class AppIA(ctk.CTk):
    def __init__(self):
        super().__init__()
        self.title("aplicacion de ia")
        self.geometry("1000x500")
        self.resizable(True, True)
        
        #variables para almacenar datos, modelos
        self.df = None
        self.modelo_regresion = RegresionLinealManual()
        self.modelo_knn = KNNManual()
        self.archivo_actual = None

        #colores pesonalizados para la interfaz
        self.color_primario = "#1E3A8A"
        self.color_secundario = "#3B82F6"
        self.color_acento = "#10B981"
        
        #configuraciond el diseño de ventana (barra lateral y area principal)
        self.grid_columnconfigure(0, weight=0, minsize=280)
        self.grid_columnconfigure(1, weight=1)
        self.grid_rowconfigure(0, weight=1)

        #barra lateral botoner para cargar csv y elegir modulo
        self.sidebar = ctk.CTkFrame(self, width=200)
        self.sidebar.grid(row=0, column=0, sticky="nsew", padx=10, pady=10)
        
        ctk.CTkLabel(self.sidebar, text="menu", font=("Arial", 18, "bold")).pack(pady=20)
        
        ctk.CTkButton(self.sidebar, text="cargar csv", command=self.cargar_csv).pack(pady=10, padx=20)
        ctk.CTkButton(self.sidebar, text="regresion lineal", command=self.vista_regresion).pack(pady=10, padx=20)
        ctk.CTkButton(self.sidebar, text="knn", command=self.vista_knn).pack(pady=10, padx=20)

        #contenido principal de la ventana (mostrar resultados, graficos, etc)
        self.main_frame = ctk.CTkFrame(self)
        self.main_frame.grid(row=0, column=1, sticky="nsew", padx=10, pady=10)
        
        self.label_info = ctk.CTkLabel(self.main_frame, text="cargue un csv para iniciar", font=("Arial", 14, "bold"))
        self.label_info.pack(pady=20)

    def cargar_csv(self):
        ruta = filedialog.askopenfilename(filetypes=[("archivos CSV", "*.csv")])
        if ruta:
            self.df = pd.read_csv(ruta)
            messagebox.showinfo("ok", f"datos cargados: {self.df.shape[0]} filas.")
            self.label_info.configure(text=f"archivo cargado correctamente: {ruta.split('/')[-1]}")

    def limpiar_frame(self):
        for widget in self.main_frame.winfo_children():
            widget.destroy()

    #modulo de regresion lineal
    def vista_regresion(self):
        if self.df is None: 
            messagebox.showerror("error", "cargar un csv"); return
        
        self.limpiar_frame()
        ctk.CTkLabel(self.main_frame, text="regresion lineal simple", font=("Arial", 16, "bold")).pack(pady=10)

        #asumir que el csv tiene 2 columnas: X e Y
        X = self.df.iloc[:, 0].values
        Y = self.df.iloc[:, 1].values
        self.modelo_regresion.entrenar(X, Y)

        #mostrar resultados de la regresion (linea de tendencia, mse)
        res_text = f"ecuacion: Y = {self.modelo_regresion.m:.2f}X + {self.modelo_regresion.b:.2f}\nMSE: {self.modelo_regresion.mse:.4f}"
        ctk.CTkLabel(self.main_frame, text=res_text, font=("consolas", 14)).pack(pady=5)

        #entrada para predecir nuevo valor de Y dado X
        self.entry_x = ctk.CTkEntry(self.main_frame, placeholder_text="ingrese valor de X")
        self.entry_x.pack(pady=5)
        ctk.CTkButton(self.main_frame, text="predecir y", command=self.predecir_regresion).pack(pady=5)
        self.lbl_pred_res = ctk.CTkLabel(self.main_frame, text="prediccion: -", font=("Arial", 14, "bold"))
        self.lbl_pred_res.pack()

        #mostrar grafico de la regresion (puntos reales y linea de tendencia) usando matplotlib integrado en tkinter
        self.mostrar_grafico_regresion(X, Y)

    def predecir_regresion(self):
        val = float(self.entry_x.get())
        res = self.modelo_regresion.predecir(val)
        self.lbl_pred_res.configure(text=f"predicción: {res:.2f}")

    def mostrar_grafico_regresion(self, X, Y):
        fig, ax = plt.subplots(figsize=(5, 4), dpi=100)
        ax.scatter(X, Y, color='green', label='datos')
        ax.plot(X, self.modelo_regresion.predecir(X), color='yellow', label='linea de tendencia')
        ax.set_title("regresion lineal simple")
        
        #integrar grafico en tkinter
        canvas = FigureCanvasTkAgg(fig, master=self.main_frame)
        canvas.draw()
        canvas.get_tk_widget().pack(pady=10, fill="both", expand=True)

    #modulo knn
    def vista_knn(self):
        if self.df is None: 
            messagebox.showerror("error", "cargar un csv"); return
        
        #limpiar el frame para mostrar opciones de knn
        self.limpiar_frame()
        ctk.CTkLabel(self.main_frame, text="knn", font=("Arial", 16, "bold")).pack(pady=10)

        #entrada para elegir k 
        ctk.CTkLabel(self.main_frame, text="valor de K:").pack()
        self.entry_k = ctk.CTkEntry(self.main_frame)
        self.entry_k.insert(0, "3")
        self.entry_k.pack()

        #entrada para elegir punto a clasificar 
        self.entry_p1 = ctk.CTkEntry(self.main_frame, placeholder_text="valor predicho 1")
        self.entry_p1.pack(pady=5)
        self.entry_p2 = ctk.CTkEntry(self.main_frame, placeholder_text="valor predicho 2")
        self.entry_p2.pack(pady=5)

        #boton para ejecutar knn y mostrar clase asignada
        ctk.CTkButton(self.main_frame, text="clasificar punto", command=self.ejecutar_knn).pack(pady=10)
        self.lbl_knn_res = ctk.CTkLabel(self.main_frame, text="clase asignada: -", font=("Arial", 12, "bold"))
        self.lbl_knn_res.pack()

        #grafico de los datos y clases usando matplotlib integrado en tkinter
        self.mostrar_grafico_knn()

    def ejecutar_knn(self):
        k = int(self.entry_k.get())
        X = self.df.iloc[:, 0:2].values #tomar las primeras 2 columnas como atributos
        Y = self.df.iloc[:, 2].values   #tomar la 3era columna como clase
        
        punto = np.array([float(self.entry_p1.get()), float(self.entry_p2.get())])
        
        #ejecutar knn con el modelo manual definido en modelos.py
        self.modelo_knn.k = k
        self.modelo_knn.entrenar(X, Y)
        clase = self.modelo_knn.predecir(punto)
        
        #mostrar clase asignada
        self.lbl_knn_res.configure(text=f"clase asignada: {clase}")

    def mostrar_grafico_knn(self):
        fig, ax = plt.subplots(figsize=(5, 4), dpi=100) #crear figura y eje para graficar
        X = self.df.iloc[:, 0].values #atributo 1 esta en la primera columna
        Y = self.df.iloc[:, 1].values #atributo 2 esta en la segunda columna
        Clase = self.df.iloc[:, 2] #asumir que la clase esta en la 3era columna
        
        scatter = ax.scatter(X, Y, c=pd.factorize(Clase)[0], cmap='viridis') #graficar puntos coloreados por clase
        ax.set_title("knn distribucion de clases")

        #integrar grafico en tkinter    
        canvas = FigureCanvasTkAgg(fig, master=self.main_frame)
        canvas.draw()
        canvas.get_tk_widget().pack(pady=10, fill="both", expand=True)

if __name__ == "__main__": #punto de entrada de la aplicacion
    app = AppIA()
    app.mainloop()