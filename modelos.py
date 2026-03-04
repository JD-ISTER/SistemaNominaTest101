import numpy as np

#clase para regresion lineal manual (sin usar librerias de ML)
class RegresionLinealManual:
    def __init__(self):
        self.m = 0  #pendiente de la recta
        self.b = 0  #intercepto de la recta
        self.mse = 0 

    #datos de entrenamiento (x,y)
    def entrenar(self, x, y):
        n = len(x)
        #calculo manual de minimos Cuadrados
        sum_x = np.sum(x)
        sum_y = np.sum(y)
        sum_xy = np.sum(x * y)
        sum_xx = np.sum(x**2)

        #calculo de pendiente (m) e intercepto (b)
        self.m = (n * sum_xy - sum_x * sum_y) / (n * sum_xx - sum_x**2)
        self.b = (sum_y - self.m * sum_x) / n
        
        #calculo del error cuadratico medio (MSE)
        predicciones = self.m * x + self.b
        self.mse = np.mean((y - predicciones)**2) 

    def predecir(self, x_nuevo):
        return self.m * x_nuevo + self.b

#clase para knn manual (sin usar librerias de ML)
class KNNManual:
    def __init__(self, k=3): #k numero a considerar para la prediccion
        self.k = k 

    #datos de entrenamiento
    def entrenar(self, X_train, y_train):
        self.X_train = np.array(X_train)
        self.y_train = np.array(y_train)

    #distancia euclidiana entre dos puntos (p1 y p2) en un espacio n-dimensional    
    def distancia_euclidiana(self, p1, p2):
        # formula: sqrt(sum((x1 - x2)^2))
        return np.sqrt(np.sum((p1 - p2)**2))

    #prediccion de clase para un nuevo punto dado los datos de entrenamiento y el valor de k
    def predecir(self, punto_nuevo):
        #calcular distancias entre el nuevo punto y todos los puntos de entrenamiento
        distancias = [self.distancia_euclidiana(punto_nuevo, x) for x in self.X_train]
        
        #obtener los indices de k mas cercanos
        k_indices = np.argsort(distancias)[:self.k]
        
        #obtener etiquetas de los k mas cercanos
        k_etiquetas = [self.y_train[i] for i in k_indices]
        
        #clase mas frecuente (moda)
        clase_predicha = max(set(k_etiquetas), key=k_etiquetas.count)
        return clase_predicha