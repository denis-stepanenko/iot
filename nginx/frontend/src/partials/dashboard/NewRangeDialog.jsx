import React, { useState } from 'react'
import { Button, Dialog, DialogTitle, TextField, DialogActions } from '@mui/material'
import DialogContent from '@mui/material/DialogContent';
import Alert from '@mui/material/Alert';

export default function NewRangeDialog({ items, onAdd }) {
  const [open, setOpen] = useState(false);
  const [commonError, setCommonError] = useState('')
  const [nameError, setNameError] = useState('')
  const [topicError, setTopicError] = useState('')
  const [minError, setMinError] = useState('')
  const [maxError, setMaxError] = useState('')
  const [stepError, setStepError] = useState('')

  const handleClickOpen = () => {
    setOpen(true);
    setCommonError('')
    setNameError('');
    setTopicError('');
    setMinError('');
    setMaxError('');
    setStepError('');
  };

  const handleClose = () => {
    setOpen(false);
  };


  return (
    <>
      <Button variant="text" onClick={handleClickOpen}>Добавить регулятор</Button>

      <Dialog
        open={open}
        onClose={handleClose}
        PaperProps={{
          component: 'form',
          onSubmit: (event) => {
            event.preventDefault();
            const formData = new FormData(event.currentTarget);
            const formJson = Object.fromEntries(formData.entries());

            const name = formJson.name;
            const topic = formJson.topic;
            const min = Number(formJson.min);
            const max = Number(formJson.max);
            const step = Number(formJson.step);

            setCommonError('')
            setNameError('');
            setTopicError('');
            setMinError('');
            setMaxError('');
            setStepError('');

            let hasErrors = false;

            if (name === '') {
              setNameError('Введите название');
              hasErrors = true;
            }

            if (topic === '') {
              setTopicError('Введите тему');
              hasErrors = true;
            }

            if (max === 0) {
              setMaxError('Максимум должен быть больше нуля');
              hasErrors = true;
            }

            if (max <= min) {
              setMaxError('Максимум должен быть больше чем минимум');
              hasErrors = true;
            }

            if (min < 0) {
              setMinError('Минимум должен быть больше или равен нулю');
              hasErrors = true;
            }

            if (items.some(x => x.topic === topic)) {
              setCommonError('Элемент с таким названием или темой уже существует');
              hasErrors = true;
            }

            if (items.some(x => x.name === name)) {
              setCommonError('Элемент с таким названием или темой уже существует');
              hasErrors = true;
            }


            if (step < 0) {
              setStepError('Шаг должен быть больше или равен нулю');
              hasErrors = true;
            }

            if (!hasErrors) {
              onAdd({ $type: 'range', title: name, topic: topic, value: 0, min: min, max: max, step: step });

              handleClose();
            }


          },
        }}
      >
        <DialogTitle>Новый регулятор</DialogTitle>
        <DialogContent>
          {commonError && <Alert severity="error">{commonError}</Alert>}

          <TextField
            autoFocus
            margin="dense"
            id="name"
            name="name"
            label="Название"
            type="text"
            fullWidth
            variant="standard"
            {...nameError === '' ? {} : { helperText: nameError, error: true }}
          />

          <TextField
            margin="dense"
            id="topic"
            name="topic"
            label="Тема"
            type="text"
            fullWidth
            variant="standard"
            {...topicError === '' ? {} : { helperText: topicError, error: true }}
          />

          <TextField
            margin="dense"
            id="min"
            name="min"
            label="Минимум"
            type="number"
            fullWidth
            variant="standard"
            {...minError === '' ? {} : { helperText: minError, error: true }}
          />

          <TextField
            margin="dense"
            id="max"
            name="max"
            label="Максимум"
            type="number"
            fullWidth
            variant="standard"
            {...maxError === '' ? {} : { helperText: maxError, error: true }}
          />

          <TextField
            margin="dense"
            id="step"
            name="step"
            label="Шаг"
            type="number"
            fullWidth
            variant="standard"
            {...stepError === '' ? {} : { helperText: stepError, error: true }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={handleClose}>Отмена</Button>
          <Button type="submit">Добавить</Button>
        </DialogActions>
      </Dialog>
    </>
  )
}
