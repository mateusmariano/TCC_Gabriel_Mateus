﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class CoordHomoController : MonoBehaviour {

	public List<Nodes> xnodes = new List<Nodes>();
	public List<Nodes> finalnodes;
	public List<Nodes> finalnodescalc;
	public Material mat;
	public GameObject nodeobject;
	public List<GameObject> nosvalues;
	public GameObject[] nosvaluesgo;
	public Text[] matrizvaluestxt,dxdy,textstochange;
	public int[] matrizvalues,tmatriz;
	public int[,] matriz = new int[3,3];
	public int[,] matrizop = new int[3,3];
	public int[,] matrizopesc = new int[3,3];
	public int[,] matrizresulaux = new int[3,3];
	public int[,] matrizresul = new int[3,3];
	public GameObject[] oppanels;
	public GameObject cam;
	public Transform target, grafoTransform;
	int[] pivo = new int[4];
	int pointcontrol;
	bool can,now, translacao;

	#region Debug
	public Text tmatriztxt; 
	public Text matriztxt; 
	public Text matrizresultxt; 
	public DebugController debug;

	#endregion

	// Use this for initialization
	void Start () {
		CriarPlano ();
	}

	void CriarPlano(){
		for(int y = -5; y < 6; y ++){
			for(int x = -5; x < 6; x ++){
				xnodes.Add(new Nodes() {
					childrens = xnodes.Where(node => Random.value > 0f).ToList(),
					pos = new Vector3(x,y,0),
					vel = Vector3.zero
				});
			}
		}
		foreach(var node in xnodes){
			Points(node.pos,node.pos.x,node.pos.y);
		}
	}

	void Points(Vector3 points,float value,float yvalue){
		GameObject inst = Instantiate(nodeobject,points, Quaternion.identity) as GameObject;
		inst.transform.parent = grafoTransform;
		if(pointcontrol>54 && pointcontrol<66){
			inst.GetComponentInChildren<Text>().text = value.ToString();
		}
		if( NodeNoEixo(pointcontrol)){
			inst.GetComponentInChildren<Text>().text = yvalue.ToString();

		}
		nosvalues.Add(inst);
		pointcontrol++;
		if(pointcontrol >= xnodes.Count){
			ListaToGameObject();
		}
	}

	void ListaToGameObject(){
		for(int i = 0; i < nosvalues.Count;i++){
			nosvaluesgo[i] = nosvalues.ElementAt(i);
		}
	}

		#region render
	void OnPostRender(){
		Lines();
		Triangles();
	}
	void Lines(){
		GL.Begin(GL.LINES);
		mat.SetPass(0);

		for(int x = 0; x < xnodes.Count-1;x++){
			if(x != xnodes.Count){
				Vector3 myposx,myposy;
				myposx = xnodes.ElementAt(x+1).pos;
				myposy = xnodes.ElementAt(x).pos;
				GL.Color(Color.red);
				myposx.x--;
				GL.Vertex(myposx);
				GL.Color(Color.red);
				myposy.y++;
				GL.Vertex(myposy);
			}		
		}
		GL.End();
	}
	void Triangles(){
		GL.Begin(GL.TRIANGLES);
		if(can){
			if(!now){
				GL.Color(Color.green);
				GL.Vertex(finalnodes.ElementAt(0).pos);
				GL.Color(Color.blue);
				GL.Vertex(finalnodes.ElementAt(1).pos);
				GL.Color(Color.red);
				
				GL.Vertex(finalnodes.ElementAt(2).pos);
				
			}else{
				GL.Color(Color.green);
				GL.Vertex(finalnodescalc.ElementAt(0).pos);
				GL.Vertex(finalnodescalc.ElementAt(1).pos);
				GL.Vertex(finalnodescalc.ElementAt(2).pos);

				if(!translacao){
					GL.Color(Color.red);
					GL.Vertex(finalnodes.ElementAt(0).pos);
					GL.Color(Color.red);
					GL.Vertex(finalnodes.ElementAt(1).pos);
					GL.Color(Color.red);
					
					GL.Vertex(finalnodes.ElementAt(2).pos);
				}
				
			}
		}
		GL.End();
	}

	#endregion
	  

	public void CriarMatriz(){
		if(TemErroMatriz ()){
			return;
		}

		finalnodes = new List<Nodes>();
		finalnodescalc = new List<Nodes>();
		#region create
		for(int i = 0; i< 6;i++){
			matrizvalues[i] = System.Int32.Parse(matrizvaluestxt[i].text);
		}
		for(int k = 0; k < 6; k +=2){
			finalnodes.Add(new Nodes(){childrens = finalnodes.Where(node => Random.value > 0.4f).ToList(),pos = new Vector3(matrizvalues[k],matrizvalues[k+1],0),vel = Vector3.zero});
		}
		can = true;

		#endregion
		oppanels[0].SetActive(true);
	}
	#region translacao
	public void Translacao(int mataux){
		if(TemErroMatriz () || TemErroXY ()){
			return;
		}
		textstochange[0].text = " M'ob = Mob * T(dx,dy)";
		#region matrizvalues
		matriz[0,0] = matrizvalues[0];
		matriz[0,1] = matrizvalues[1];
		matriz[0,2] = 1;
		matriz[1,0] = matrizvalues[2];
		matriz[1,1] = matrizvalues[3];
		matriz[1,2] = 1;
		matriz[2,0] = matrizvalues[4];
		matriz[2,1] = matrizvalues[5];
		matriz[2,2] = 1;

		
		matrizop[0,0] = 1;
		matrizop[0,1] = 0;
		matrizop[0,2] = 0;
		matrizop[1,0] = 0;
		matrizop[1,1] = 1;
		matrizop[1,2] =	0;
		matrizop[2,0] = System.Convert.ToInt32(dxdy[0].text);
		matrizop[2,1] = System.Convert.ToInt32(dxdy[1].text);
		matrizop[2,2] = 1;
		#endregion
		#region calc
		for(int i = 0; i < 3; i ++){
			for(var j = 0 ; j < 3; j ++){
				for(var x = 0; x < 3; x ++){
					mataux += matriz[i,x] * matrizop[x,j];
				}
				matrizresul[i,j] = mataux;
				mataux = 0;
			}
		}
		#endregion

		FormataMatrizDebug ();

		for(int  i = 0; i <= finalnodes.Count; i ++){
			finalnodes.RemoveAt(i);
		}
		if(finalnodescalc.Count != 0){
			for(int  i = 0; i <= finalnodescalc.Count; i ++){
				finalnodescalc.RemoveAt(i);
			}
		}
		finalnodescalc.Add(new Nodes(){childrens = finalnodescalc.Where(node => Random.value > 0.4f).ToList(),pos = new Vector3(matrizresul[0,0],matrizresul[0,1],0),vel = Vector3.zero});
		finalnodescalc.Add(new Nodes(){childrens = finalnodescalc.Where(node => Random.value > 0.4f).ToList(),pos = new Vector3(matrizresul[1,0],matrizresul[1,1],0),vel = Vector3.zero});
		finalnodescalc.Add(new Nodes(){childrens = finalnodescalc.Where(node => Random.value > 0.4f).ToList(),pos = new Vector3(matrizresul[2,0],matrizresul[2,1],0),vel = Vector3.zero});
		Debug.Log(finalnodescalc.Count);
		
		now = true;
		translacao = true;

	}
	#endregion  
	#region escalonamento
	public void Escalonamento(int mataux){
		if(TemErroMatriz () || TemErroXY ()){
			return;
		}

		textstochange[0].text = " M'ob = Mob * S(ex,ey)";
		#region matrizvalues
		matriz[0,0] = matrizvalues[0];
		matriz[0,1] = matrizvalues[1];
		matriz[0,2] = 1;
		matriz[1,0] = matrizvalues[2];
		matriz[1,1] = matrizvalues[3];
		matriz[1,2] = 1;
		matriz[2,0] = matrizvalues[4];
		matriz[2,1] = matrizvalues[5];
		matriz[2,2] = 1;

		#endregion
		#region calc
		// a variavel "pivo[]" contem 4 int, onde as 2 primeiras sao as coordenadas do pivo, e os 2 ultimos valores sao a coordenada de translaçao do triangulo para a origem;
		pivo[0] = matriz[0,0];
		pivo[1] = matriz[0,1];

		pivo[2] = pivo[0] * -1;
		pivo[3] = pivo[1] * -1;
		//matrizop recebe agora os valores das coordenadas para a origem
		matrizop[0,0] = 1;
		matrizop[0,1] = 0;
		matrizop[0,2] = 0;
		matrizop[1,0] = 0;
		matrizop[1,1] = 1;
		matrizop[1,2] =	0;
		matrizop[2,0] = pivo[2];
		matrizop[2,1] = pivo[3];
		matrizop[2,2] = 1;

		// o codigo abaixo faz a 1° multiplicacao de matrizes,onde "matriz" sao os pontos do triangulo e "matrizop" e a matriz com as coordenadas do pivo modificado
		for(int i = 0; i < 3; i ++){
			for(var j = 0 ; j < 3; j ++){
				for(var x = 0; x < 3; x ++){
					mataux += matriz[i,x] * matrizop[x,j];
				}
				matrizopesc[i,j] = mataux;
				mataux = 0;
			}
		}
		//matrizop e modificada para receber as entradas de escalonamento em sx e sy;
		matrizop[0,0] = System.Convert.ToInt32(dxdy[0].text);
		matrizop[0,1] = 0;
		matrizop[0,2] = 0;
		matrizop[1,0] = 0;
		matrizop[1,1] = System.Convert.ToInt32(dxdy[1].text);
		matrizop[1,2] =	0;
		matrizop[2,0] = 0;
		matrizop[2,1] = 0;
		matrizop[2,2] = 1;
		//realizacao do escalonamento
		for(int i = 0; i < 3; i ++){
			for(var j = 0 ; j < 3; j ++){
				for(var x = 0; x < 3; x ++){
					mataux += matrizopesc[i,x] * matrizop[x,j];
				}
				matrizresulaux[i,j] = mataux;
				mataux = 0;
			}
		}
		//matrizop recebe os valores da coordenada da matriz original, o ponto original do pivo.
		matrizop[0,0] = 1;
		matrizop[0,1] = 0;
		matrizop[0,2] = 0;
		matrizop[1,0] = 0;
		matrizop[1,1] = 1;
		matrizop[1,2] =	0;
		matrizop[2,0] = pivo[0];
		matrizop[2,1] = pivo[1];
		matrizop[2,2] = 1;
		//translacao de volta ao local original
		for(int i = 0; i < 3; i ++){
			for(var j = 0 ; j < 3; j ++){
				for(var x = 0; x < 3; x ++){
					mataux += matrizresulaux[i,x] * matrizop[x,j];
				}
				matrizresul[i,j] = mataux;
				mataux = 0;
			}
		}
		#endregion

		FormataMatrizDebug ();

		if(finalnodescalc.Count != 0){
			for(int  i = 0; i <= finalnodescalc.Count; i ++){
				finalnodescalc.RemoveAt(i);
			}
		}
		finalnodescalc.Add(new Nodes(){childrens = finalnodescalc.Where(node => Random.value > 0.4f).ToList(),pos = new Vector3(matrizresul[0,0],matrizresul[0,1],0),vel = Vector3.zero});
		finalnodescalc.Add(new Nodes(){childrens = finalnodescalc.Where(node => Random.value > 0.4f).ToList(),pos = new Vector3(matrizresul[1,0],matrizresul[1,1],0),vel = Vector3.zero});
		finalnodescalc.Add(new Nodes(){childrens = finalnodescalc.Where(node => Random.value > 0.4f).ToList(),pos = new Vector3(matrizresul[2,0],matrizresul[2,1],0),vel = Vector3.zero});
		Debug.Log(finalnodescalc.Count);
		
		now = true;

	}
	#endregion

	#region MatrizModelo

	public void MatrizModelo(){
		matrizvaluestxt [0].GetComponentInParent<InputField>().text = "0";
		matrizvaluestxt [1].GetComponentInParent<InputField>().text = "0";
		matrizvaluestxt [2].GetComponentInParent<InputField>().text = "0";
		matrizvaluestxt [3].GetComponentInParent<InputField>().text = "2";
		matrizvaluestxt [4].GetComponentInParent<InputField>().text = "2";
		matrizvaluestxt [5].GetComponentInParent<InputField>().text = "1";
		matrizvaluestxt [6].GetComponentInParent<InputField>().text = "1";
		matrizvaluestxt [7].GetComponentInParent<InputField>().text = "1";
		matrizvaluestxt [8].GetComponentInParent<InputField>().text = "1";

		CriarMatriz ();



	}
	#endregion

	bool NodeNoEixo(int pointControl){
		//os nós do eixo sempre obedecem a ordem de crescente somando 11
		int atualSequencia = 5;
		for(int i = atualSequencia; i <= 115; i += 11){
			if(pointcontrol == i){
				return true;
			}
		}

		return false;
	}

	bool TemErroMatriz(){
		for(int i = 0; i < matrizvaluestxt.Length; i ++){
			if(matrizvaluestxt[i].text == "" || !Regex.IsMatch (matrizvaluestxt[i].text, "^-?[0-9]+$")){
				debug.ShowDebug ("Os valores da matriz não  \nforam inseridos corretamente. \nInsira somente números");
				return true;
			}
		} 
		return false;
	}

	bool TemErroXY(){
		if(dxdy[0].text == "" || !Regex.IsMatch (dxdy[0].text, "^-?[0-9]+$") || dxdy[1].text == "" || !Regex.IsMatch (dxdy[1].text, "^-?[0-9]+$")) {
			debug.ShowDebug ("Os valores de X e Y não \nforam inseridos corretamente. \nInsira somente números");
			return true;
		}
		return false;
	}

	void FormataMatrizDebug(){
		LimpaMatrizDebug ();

		for(int i = 0; i < 3; i ++){
			for(int a = 0; a < 3;a++){
				matriztxt.text += ("\t" + matriz[i,a].ToString());
			}
			matriztxt.text +=("\n");
		}
		for(int i = 0; i < 3; i ++){
			for(int a = 0; a < 3;a++){
				tmatriztxt.text += ("\t" + matrizop[i,a].ToString());
			}
			tmatriztxt.text +=("\n");
		}
		for(int i = 0; i < 3; i ++){
			for(int a = 0; a < 3;a++){
				matrizresultxt.text += ("\t" + matrizresul[i,a].ToString());
			}
			matrizresultxt.text +=("\n");
		}
	}
	void LimpaMatrizDebug(){
		matriztxt.text = "";
		tmatriztxt.text = "";
		matrizresultxt.text = "";
	}
}
