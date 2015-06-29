function [ decision ] = albumDecideResponseSVM( userWorkSpace, kernel, numOfParameters )

fileToDecide = strcat(userWorkSpace,'\\Decide.txt');
profilePath = strcat(userWorkSpace,'\\profile');
profilePath = strcat(profilePath, kernel);
profilePath = strcat(profilePath, '.mat');
resultFilePath = strcat(userWorkSpace,'\\Result.txt');


load(profilePath,'svmStruct');

Sample = csvread(fileToDecide);
Sample = Sample(1:1, 1:numOfParameters);

decision = svmclassify(svmStruct, Sample);

SampleScaleShift = bsxfun(@plus,Sample,svmStruct.ScaleData.shift);
Sample = bsxfun(@times,SampleScaleShift,svmStruct.ScaleData.scaleFactor);
sv = svmStruct.SupportVectors;
alphaHat = svmStruct.Alpha;
bias = svmStruct.Bias;
kfun = svmStruct.KernelFunction;
kfunargs = svmStruct.KernelFunctionArgs;
f = kfun(sv, Sample, kfunargs{:})'*alphaHat(:) + bias;
Confidence = f*-1;

resultFile = fopen(resultFilePath, 'w');
fprintf(resultFile, '%d\n', decision);
fprintf(resultFile, '%d', Confidence);
fclose(resultFile);


end

