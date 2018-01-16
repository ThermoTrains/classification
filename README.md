# Train Classification

## Template Matching

This project contains a Visual Studio solution with a project to recognize train identification numbers.
Results are not promising though. So this represents of how not to do it.

See https://en.wikipedia.org/wiki/UIC_identification_marking_for_tractive_stock

## Machine Learning

This repository contains the Tensorflow models to do train detection.

Further along there might be a model to detect isolation deficiencies.

	python -m scripts.retrain `
	   --bottleneck_dir=tf_files/bottlenecks `
	   --how_many_training_steps=4000 `
	   --model_dir=tf_files/models/ `
	   --summaries_dir=tf_files/training_summaries/"$ARCHITECTURE" `
	   --output_graph=tf_files/train_graph.pb `
	   --output_labels=tf_files/train_labels.txt `
	   --architecture="$ARCHITECTURE" `
	   --image_dir=tf_files/trains
