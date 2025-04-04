//import "./App.css";
import { useState, useEffect } from "react";



const Header = () => {
  <header>
    <img src="./iconCC.jpg" width={60} height={60} alt="Icon Profile" />
  </header>
}

function Dados(){
  const initialValues ={
    username:"",
    email:"",
    password:"",
  };

  const [formValues, setFromValues] = useState(initialValues);
  const [fromErrors, setFromErrors] = useState({});
  const [isSubmit, setIsSubmit] = useState(false);

  const handleChange = (e) => {
    const {name, value} = e.target;
    setFromValues({...formValues, [name]: value});
  }

  const handleSubmit = (e) =>{
    e.preventDefault();
    setFromErrors(validate(formValues));
    setIsSubmit(true);
  }

  // Este useEffect é acionado sempre que formErrors, formValues ou isSubmit mudar.
  // Ele verifica se o formulário foi submetido e não há erros. Se for o caso, exibe os valores no console.
  useEffect(() =>{
    console.log(fromErrors);
    if(Object.keys(fromErrors).length === 0 && isSubmit){
      console.log(formValues);
    }
  }, [fromErrors, formValues, isSubmit]);

  const validate = (values) => {
    const errors = {};
    const regex = /^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/i;
    const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*[\W_])$/; // Pelo menos uma letra maiúscula, uma letra minuscula, um símbolo especial

    if(!values.username){
      errors.username = "Nome é necessário!";
    }
    if(!values.email){
      errors.email = "Email é necessário";
    }else if (!regex.test(values.email)){
      errors.email = "Não é válido para o formato de email!";
    }
    if(!values.password){
      errors.password = "É necessário password";
    }else if (values.password.length < 10){
      errors.password = "Password tem que ter mais de 10 caracteres";
    }else if (values.password.length > 16){
      errors.password = "Password não pode ter mais de 16 caracteres";
    }else if (!passwordRegex.test(values.password)){
      errors.password = "Password deve conter pelo menos uma letra minuscula, uma maiuscula e um simbolo especial"
    }
    return errors;
  };

  return(
    <>
      <div className="back"></div>
      <div className="container"></div>
        {Object.keys(fromErrors).length === 0 && isSubmit ? (
          <div className="message success">
            Registo foi sucedido
          </div>
        ) : (
          console.log("Dados inseridos", formValues)
        )}

        <form onSubmit={handleSubmit}>
          
        </form>
    </>
  )


}

function Registar(){
  return(
    <Header />,
    <Dados/>
  )



}


export default Registar